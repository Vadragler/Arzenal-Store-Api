using Arzenal.Dto.DTOs.AppFileDto;
using Arzenal.Dto.DTOs.FileDto;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Interfaces;
using ArzenalStoreInfrastructure.Configurations;
using ArzenalStoreInfrastructure.Data;
using Microsoft.Extensions.Options;

namespace Arzenal.Store.Api.Service.Services.AppFilesService
{
    public class AppFilesService : IAppFilesService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _storagePath;

        public AppFilesService(ApplicationDbContext context, IOptions<StorageSettings> storageSettings) 
        {
            _context = context;
            _storagePath = storageSettings.Value.AppFilesPath;
        }

        public async Task<string> UploadFile(UploadFileDto dto,Stream input)
        {
            if (input == null || !input.CanRead)
                throw new ValidationException("Flux invalide.");

            var app = await _context.Apps.FindAsync(dto.AppId);
            if (app == null)
                throw new NotFoundException("Aucune application trouvée.");

            var versionSafe = dto.Version.Replace(".", "_");
            var dir = Path.Combine(_storagePath, dto.AppId.ToString());

            string filePath;

            if (dto.Type == "icon")
            {
                Directory.CreateDirectory(dir);
                foreach (var oldIcon in Directory.GetFiles(dir, "icon.*"))
                    File.Delete(oldIcon);

                var ext = Path.GetExtension(dto.FileName);
                filePath = Path.Combine(dir, "icon" + ext);
            }
            else if (dto.Type == "app")
            {
                if (string.IsNullOrWhiteSpace(dto.Platform))
                    throw new ValidationException("La plateforme doit être spécifiée.");

                var platformDir = Path.Combine(dir,versionSafe, dto.Platform);
                Directory.CreateDirectory(platformDir);

                var ext = Path.GetExtension(dto.FileName);
                filePath = Path.Combine(platformDir, "app" + ext);
            }
            else
            {
                filePath = Path.Combine(dir, dto.FileName);
            }

            await using var output = File.Create(filePath);
            await input.CopyToAsync(output);

            return filePath;
        }

        public async Task<AppFileStreamResult> Download(Guid appId)
        {
            var appDir = await GetFilePath(appId);

            var files = Directory.GetFiles(appDir).Where(f => !f.EndsWith(".ico")).ToArray();

            if (files.Length == 0)
                throw new NotFoundException("Fichier introuvable.");

            var filePath = files[0];

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
                Path.GetFileName(filePath),
                "application/octet-stream"
            );
        }

        public async Task <FileResultDto> GetAppIcon(Guid appId)
        {
            var appDir = await GetFilePath(appId);
            var iconFiles = Directory.GetFiles(appDir, "icon.*");
            if (iconFiles.Length == 0)
                throw new NotFoundException("Icône introuvable.");
            var iconPath = iconFiles[0];
            var contentType = iconPath.EndsWith(".png") ? "image/png" :
                              iconPath.EndsWith(".jpg") || iconPath.EndsWith(".jpeg") ? "image/jpeg" :
                              iconPath.EndsWith(".ico") ? "image/x-icon" :
                              "application/octet-stream";
            return new FileResultDto
            {
                FileName = Path.GetFileName(iconPath),
                ContentType = contentType,
                Stream = File.OpenRead(iconPath)
            };
        }

        public async Task<bool> DeleteFile(Guid appId)
        {
            var app = await _context.Apps.FindAsync(appId);
            if (app == null)
                throw new NotFoundException("Aucune application trouvée.");

            var appDir = Path.Combine(_storagePath, appId.ToString());
            Directory.Delete(appDir, true);
            return true;
        }

        public async Task<string> GetFilePath(Guid appId)
        {
            var app = await _context.Apps.FindAsync(appId);
            if (app == null)
                throw new NotFoundException("Aucune application trouvée.");

            var filePath = Path.Combine(_storagePath, appId.ToString());
            if (!Directory.Exists(filePath))
                throw new NotFoundException("Fichier introuvable.");

            return filePath;
        }
    }
}
