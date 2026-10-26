using Arzenal.Dto.DTOs.AppFileDto;
using Arzenal.Dto.DTOs.FileDto;
using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Interfaces;
using ArzenalStoreInfrastructure.Configurations;
using ArzenalStoreInfrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Arzenal.Store.Api.Service.Services.AppFilesService
{
    public class AppFilesService : IAppFilesService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _storagePath;
        private readonly string _storagePathPrefix;
        private readonly long _maxUploadBytes;
        private readonly bool _requireMalwareScan;
        private readonly IFileScanner? _fileScanner;

        private static readonly Regex VersionPattern = new(
            @"^[A-Za-z0-9][A-Za-z0-9._-]{0,63}$",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly HashSet<string> SupportedPlatforms = new(StringComparer.OrdinalIgnoreCase)
        {
            "windows", "linux", "macos", "android", "ios"
        };
        private static readonly HashSet<string> IconExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".webp", ".ico"
        };
        private static readonly HashSet<string> ApplicationExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".exe", ".msi", ".msix", ".apk", ".ipa", ".zip"
        };

        public AppFilesService(
            ApplicationDbContext context,
            IOptions<StorageSettings> storageSettings,
            IFileScanner? fileScanner = null)
        {
            _context = context;
            var configuredPath = storageSettings.Value.AppFilesPath;
            if (string.IsNullOrWhiteSpace(configuredPath))
                throw new InvalidOperationException("Le répertoire Storage:AppFilesPath doit être configuré.");

            _storagePath = Path.GetFullPath(configuredPath);
            if (string.Equals(_storagePath, Path.GetPathRoot(_storagePath), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Storage:AppFilesPath ne peut pas être la racine d'un disque.");

            _storagePathPrefix = _storagePath.EndsWith(Path.DirectorySeparatorChar)
                ? _storagePath
                : _storagePath + Path.DirectorySeparatorChar;
            _maxUploadBytes = storageSettings.Value.MaxUploadBytes;
            if (_maxUploadBytes <= 0)
                throw new InvalidOperationException("Storage:MaxUploadBytes doit être supérieur à zéro.");
            _requireMalwareScan = storageSettings.Value.RequireMalwareScan;
            _fileScanner = fileScanner;
        }

        public async Task<string> UploadFile(UploadFileDto dto,Stream input)
        {
            if (input == null || !input.CanRead)
                throw new ValidationException("Flux invalide.");

            var app = await _context.Apps.FindAsync(dto.AppId);
            if (app == null)
                throw new NotFoundException("Aucune application trouvée.");

            var type = dto.Type?.Trim().ToLowerInvariant();
            if (type is not ("icon" or "app"))
                throw new ValidationException("Type de fichier invalide.");

            ValidateFileName(dto.FileName);
            var extension = Path.GetExtension(dto.FileName).ToLowerInvariant();
            ValidateExtension(type, extension);

            var appDirectory = GetPathInsideStorage(dto.AppId.ToString());
            var version = type == "app" ? NormalizeVersion(dto.Version) : null;
            var platform = type == "app" ? NormalizePlatform(dto.Platform) : null;
            var destinationPath = type == "icon"
                ? GetPathInsideStorage(dto.AppId.ToString(), "icon" + extension)
                : GetPathInsideStorage(dto.AppId.ToString(), version!, platform!, "app" + extension);

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            var temporaryPath = GetPathInsideStorage(dto.AppId.ToString(), $".upload-{Guid.NewGuid():N}.tmp");

            try
            {
                await CopyAndValidateAsync(input, temporaryPath, type, extension);
                if (_requireMalwareScan)
                {
                    if (_fileScanner is null)
                        throw new InvalidOperationException("Le scan antivirus est requis mais aucun scanner n'est disponible.");

                    await _fileScanner.ScanAsync(temporaryPath);
                }

                if (type == "icon")
                {
                    foreach (var oldIcon in Directory.GetFiles(appDirectory, "icon.*", SearchOption.TopDirectoryOnly))
                    {
                        if (!string.Equals(oldIcon, destinationPath, StringComparison.OrdinalIgnoreCase))
                            File.Delete(oldIcon);
                    }
                }

                File.Move(temporaryPath, destinationPath, overwrite: true);
                await UpsertArtifactAsync(
                    dto.AppId,
                    type,
                    version,
                    platform,
                    dto.FileName,
                    destinationPath);
                return destinationPath;
            }
            catch
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
                throw;
            }
        }

        public async Task<AppFileStreamResult> Download(Guid appId, string version, string platform)
        {
            var normalizedVersion = NormalizeVersion(version);
            var normalizedPlatform = NormalizePlatform(platform);
            var locator = GetArtifactLocator(normalizedVersion, normalizedPlatform);
            var artifact = await _context.AppArtifacts
                .AsNoTracking()
                .SingleOrDefaultAsync(candidate =>
                    candidate.AppId == appId &&
                    candidate.Type == "app" &&
                    candidate.Locator == locator);

            if (artifact is null)
                throw new NotFoundException("Fichier introuvable.");

            var filePath = GetPathFromRelativePath(artifact.RelativePath);
            if (!File.Exists(filePath))
                throw new NotFoundException("Fichier introuvable.");

            var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 64 * 1024,
                useAsync: true
            );

            return new AppFileStreamResult(
                stream,
                artifact.OriginalFileName,
                artifact.ContentType
            );
        }

        public async Task <FileResultDto> GetAppIcon(Guid appId)
        {
            if (!await _context.Apps.AnyAsync(app => app.Id == appId))
                throw new NotFoundException("Aucune application trouvée.");

            var artifact = await _context.AppArtifacts
                .AsNoTracking()
                .SingleOrDefaultAsync(candidate => candidate.AppId == appId && candidate.Type == "icon");
            if (artifact is null)
                throw new NotFoundException("Icône introuvable.");

            var iconPath = GetPathFromRelativePath(artifact.RelativePath);
            if (!File.Exists(iconPath))
                throw new NotFoundException("Icône introuvable.");

            return new FileResultDto
            {
                FileName = artifact.OriginalFileName,
                ContentType = artifact.ContentType,
                Stream = File.OpenRead(iconPath)
            };
        }

        public async Task<bool> DeleteFile(Guid appId)
        {
            var app = await _context.Apps.FindAsync(appId);
            if (app == null)
                throw new NotFoundException("Aucune application trouvée.");

            var appDir = GetPathInsideStorage(appId.ToString());
            var artifacts = await _context.AppArtifacts
                .Where(artifact => artifact.AppId == appId)
                .ToListAsync();
            if (!Directory.Exists(appDir) && artifacts.Count == 0)
                throw new NotFoundException("Fichier introuvable.");

            if (Directory.Exists(appDir))
                Directory.Delete(appDir, true);
            _context.AppArtifacts.RemoveRange(artifacts);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GetFilePath(Guid appId)
        {
            var app = await _context.Apps.FindAsync(appId);
            if (app == null)
                throw new NotFoundException("Aucune application trouvée.");

            var filePath = GetPathInsideStorage(appId.ToString());
            if (!Directory.Exists(filePath))
                throw new NotFoundException("Fichier introuvable.");

            return filePath;
        }

        private static string NormalizePlatform(string? platform)
        {
            if (string.IsNullOrWhiteSpace(platform))
                throw new ValidationException("La plateforme doit être spécifiée.");

            var normalizedPlatform = platform.Trim().ToLowerInvariant();
            if (!SupportedPlatforms.Contains(normalizedPlatform))
                throw new ValidationException("La plateforme est invalide.");

            return normalizedPlatform;
        }

        private static string NormalizeVersion(string? version)
        {
            var normalizedVersion = version?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedVersion) ||
                normalizedVersion is "." or ".." ||
                !VersionPattern.IsMatch(normalizedVersion))
                throw new ValidationException("La version est invalide.");

            return normalizedVersion;
        }

        private string GetPathInsideStorage(params string[] components)
        {
            var path = Path.GetFullPath(Path.Combine(new[] { _storagePath }.Concat(components).ToArray()));
            if (!path.StartsWith(_storagePathPrefix, StringComparison.OrdinalIgnoreCase))
                throw new ValidationException("Chemin de fichier invalide.");

            return path;
        }

        private string GetPathFromRelativePath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathFullyQualified(relativePath))
                throw new NotFoundException("Fichier introuvable.");

            var components = relativePath.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
            return GetPathInsideStorage(components);
        }

        private async Task UpsertArtifactAsync(
            Guid appId,
            string type,
            string? version,
            string? platform,
            string originalFileName,
            string destinationPath)
        {
            var locator = type == "icon"
                ? "icon"
                : GetArtifactLocator(version!, platform!);
            var artifact = await _context.AppArtifacts.SingleOrDefaultAsync(candidate =>
                candidate.AppId == appId && candidate.Type == type && candidate.Locator == locator);
            var previousRelativePath = artifact?.RelativePath;
            var relativePath = Path.GetRelativePath(_storagePath, destinationPath);

            if (artifact is null)
            {
                artifact = new AppArtifact
                {
                    AppId = appId,
                    Type = type,
                    Locator = locator,
                    Version = version,
                    Platform = platform,
                    OriginalFileName = originalFileName,
                    RelativePath = relativePath,
                    ContentType = GetContentType(destinationPath),
                    FileSize = new FileInfo(destinationPath).Length,
                    UploadedAt = DateTime.UtcNow
                };
                _context.AppArtifacts.Add(artifact);
            }
            else
            {
                artifact.Version = version;
                artifact.Platform = platform;
                artifact.OriginalFileName = originalFileName;
                artifact.RelativePath = relativePath;
                artifact.ContentType = GetContentType(destinationPath);
                artifact.FileSize = new FileInfo(destinationPath).Length;
                artifact.UploadedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(previousRelativePath) &&
                !string.Equals(previousRelativePath, relativePath, StringComparison.OrdinalIgnoreCase))
            {
                var previousPath = GetPathFromRelativePath(previousRelativePath);
                if (File.Exists(previousPath))
                    File.Delete(previousPath);
            }
        }

        private static string GetArtifactLocator(string version, string platform) =>
            string.Concat(version, "|", platform);

        private static string GetContentType(string path) =>
            Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".webp" => "image/webp",
                ".ico" => "image/x-icon",
                _ => "application/octet-stream"
            };

        private static void ValidateFileName(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) ||
                !string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal) ||
                Path.IsPathFullyQualified(fileName) ||
                fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new ValidationException("Nom de fichier invalide.");
            }
        }

        private static void ValidateExtension(string type, string extension)
        {
            var allowed = type == "icon" ? IconExtensions : ApplicationExtensions;
            if (!allowed.Contains(extension))
                throw new ValidationException("Extension de fichier non autorisée.");
        }

        private async Task CopyAndValidateAsync(Stream input, string temporaryPath, string type, string extension)
        {
            var header = new byte[16];
            var headerLength = 0;
            while (headerLength < header.Length)
            {
                var read = await input.ReadAsync(header.AsMemory(headerLength, header.Length - headerLength));
                if (read == 0)
                    break;
                headerLength += read;
            }

            ValidateSignature(type, extension, header.AsSpan(0, headerLength));

            await using var output = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 64 * 1024,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            long total = headerLength;
            if (total > _maxUploadBytes)
                throw new ValidationException("Le fichier dépasse la taille maximale autorisée.");

            await output.WriteAsync(header.AsMemory(0, headerLength));
            var buffer = new byte[64 * 1024];
            while (true)
            {
                var read = await input.ReadAsync(buffer);
                if (read == 0)
                    break;

                total += read;
                if (total > _maxUploadBytes)
                    throw new ValidationException("Le fichier dépasse la taille maximale autorisée.");

                await output.WriteAsync(buffer.AsMemory(0, read));
            }
        }

        private static void ValidateSignature(string type, string extension, ReadOnlySpan<byte> header)
        {
            var isValid = type == "icon"
                ? extension switch
                {
                    ".png" => header.StartsWith(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
                    ".jpg" or ".jpeg" => header.StartsWith(new byte[] { 0xFF, 0xD8, 0xFF }),
                    ".ico" => header.StartsWith(new byte[] { 0x00, 0x00, 0x01, 0x00 }),
                    ".webp" => header.Length >= 12 && header[..4].SequenceEqual("RIFF"u8) && header.Slice(8, 4).SequenceEqual("WEBP"u8),
                    _ => false
                }
                : extension switch
                {
                    ".exe" => header.StartsWith("MZ"u8),
                    ".msi" => header.StartsWith(new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }),
                    ".msix" or ".apk" or ".ipa" or ".zip" => header.StartsWith("PK"u8),
                    _ => false
                };

            if (!isValid)
                throw new ValidationException("Le contenu ne correspond pas au type de fichier annoncé.");
        }
    }
}
