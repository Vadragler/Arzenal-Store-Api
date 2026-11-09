using System.IO.Compression;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;
using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.Service.Contracts.Store;
using Arzenal.Store.Api.Service.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Arzenal.Store.Api.Service.Services.Store;

public sealed partial class StoreService
{
    public async Task<StoreArtifactDto> UploadAsync(Guid releaseId, StoreArtifactRequest request,
        Stream input, string keyId, string signature, string actor, CancellationToken ct)
    {
        StoreRules.ValidateArtifact(request);
        if (!input.CanRead || storage.Value.MaxUploadBytes <= 0) throw new ValidationException("Flux invalide.");
        await using var transaction = await BeginTransactionAsync(ct);
        var release = await LockReleaseAsync(releaseId, ct);
        EnsureEditable(release);
        if (await db.AppArtifacts.AnyAsync(artifact => artifact.ReleaseId == releaseId && artifact.Platform == request.Platform &&
            artifact.Architecture == request.Architecture && artifact.PackageType == request.PackageType, ct))
            throw new DuplicateException("Variante déjà existante. Supprimer le fichier brouillon avant de le remplacer.");

        var artifactId = Guid.NewGuid();
        var temporary = StoragePath($".staging/{artifactId:N}.tmp");
        var relative = $"releases/{releaseId:N}/{artifactId:N}.{request.PackageType}";
        var destination = StoragePath(relative);
        Directory.CreateDirectory(Path.GetDirectoryName(temporary)!);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        var moved = false;
        var committed = false;
        try
        {
            long size = 0;
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            await using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 65536, true))
            {
                var buffer = new byte[65536];
                int read;
                while ((read = await input.ReadAsync(buffer, ct)) != 0)
                {
                    size += read;
                    if (size > storage.Value.MaxUploadBytes) throw new ValidationException("Fichier trop volumineux.");
                    hash.AppendData(buffer, 0, read);
                    await output.WriteAsync(buffer.AsMemory(0, read), ct);
                }
            }
            if (size == 0) throw new ValidationException("Fichier vide.");
            var digest = hash.GetHashAndReset();
            signatures.Verify(keyId, signature, digest);
            ValidatePackage(temporary, request);
            if (storage.Value.RequireMalwareScan) await scanner.ScanAsync(temporary, ct);

            var artifact = new AppArtifact
            {
                Id = artifactId,
                AppId = release.AppId,
                ReleaseId = release.Id,
                Type = "app",
                Locator = artifactId.ToString("N"),
                Version = release.Version,
                Platform = request.Platform,
                Architecture = request.Architecture,
                PackageType = request.PackageType,
                PackageId = request.PackageId,
                PackageVersion = request.PackageVersion,
                MinimumOsVersion = request.MinimumOsVersion,
                AndroidVersionCode = request.AndroidVersionCode,
                Sha256 = Convert.ToHexStringLower(digest),
                PublisherKeyId = keyId,
                PublisherSignature = signature,
                OriginalFileName = $"{request.PackageId}-{request.PackageVersion}.{request.PackageType}",
                RelativePath = relative,
                ContentType = "application/octet-stream",
                FileSize = size
            };
            File.Move(temporary, destination, overwrite: false);
            moved = true;
            db.AppArtifacts.Add(artifact);
            release.Status = "Uploaded";
            Audit(release.AppId, release.Id, artifact.Id, actor, "artifact-uploaded");
            await db.SaveChangesAsync(ct);
            if (transaction is not null) await transaction.CommitAsync(ct);
            committed = true;
            return MapArtifact(artifact);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
            if (moved && !committed && File.Exists(destination)) File.Delete(destination);
        }
    }

    public async Task<StoreReleaseDto> PublishAsync(Guid id, string actor, CancellationToken ct)
    {
        await using var transaction = await BeginTransactionAsync(ct);
        var release = await LockReleaseAsync(id, ct);
        if (release.Status == "Published") return await ReleaseAsync(id, ct);
        EnsureEditable(release);
        var app = await db.Apps.SingleAsync(app => app.Id == release.AppId, ct);
        if (app.Slug is null || !app.IsVisible) throw new DuplicateException("Configurer et activer l'application avant publication.");
        var artifacts = await db.AppArtifacts.Where(artifact => artifact.ReleaseId == id).ToListAsync(ct);
        if (artifacts.Count == 0) throw new DuplicateException("Une release doit contenir un fichier validé.");
        var history = await db.AppArtifacts.AsNoTracking().Where(artifact => artifact.AppId == release.AppId && artifact.ReleaseId != id &&
            artifact.ReleaseId != null && artifact.Release!.PublishedAt != null).ToListAsync(ct);
        foreach (var artifact in artifacts)
        {
            if (artifact.Sha256 is null || artifact.PublisherKeyId is null || artifact.PublisherSignature is null ||
                artifact.PackageType is null || artifact.PackageVersion is null || artifact.PackageId is null ||
                artifact.MinimumOsVersion is null || artifact.Architecture is null || artifact.Platform is null)
                throw new DuplicateException("Les anciens fichiers doivent être remplacés par un upload Store V1 validé.");
            var path = StoragePath(artifact.RelativePath);
            if (!File.Exists(path) || new FileInfo(path).Length != artifact.FileSize)
                throw new DuplicateException("Fichier manquant ou modifié.");
            await using var file = File.OpenRead(path);
            var hash = await SHA256.HashDataAsync(file, ct);
            if (Convert.ToHexStringLower(hash) != artifact.Sha256) throw new DuplicateException("Fichier modifié.");
            signatures.Verify(artifact.PublisherKeyId, artifact.PublisherSignature, hash);
            foreach (var previous in history.Where(previous => previous.Platform == artifact.Platform))
            {
                if (previous.PackageId != artifact.PackageId)
                    throw new DuplicateException("L'identité du paquet doit rester stable.");
                if (!StoreRules.ArchitectureMatches(previous.Architecture!, artifact.Architecture) &&
                    !StoreRules.ArchitectureMatches(artifact.Architecture, previous.Architecture)) continue;
                if (artifact.Platform == "android" ? artifact.AndroidVersionCode <= previous.AndroidVersionCode :
                    Version.Parse(artifact.PackageVersion) <= Version.Parse(previous.PackageVersion!))
                    throw new DuplicateException("La version installable doit augmenter.");
            }
        }
        release.Status = "Published";
        release.PublishedAt = DateTime.UtcNow;
        app.Version = release.Version;
        Audit(release.AppId, release.Id, null, actor, "release-published");
        await db.SaveChangesAsync(ct);
        if (transaction is not null) await transaction.CommitAsync(ct);
        return MapRelease(release);
    }

    public async Task<StoreReleaseDto> WithdrawAsync(Guid id, string actor, CancellationToken ct)
    {
        await using var transaction = await BeginTransactionAsync(ct);
        var release = await LockReleaseAsync(id, ct);
        if (release.Status != "Published") throw new DuplicateException("Seule une release publiée peut être retirée.");
        release.Status = "Withdrawn";
        Audit(release.AppId, release.Id, null, actor, "release-withdrawn");
        await db.SaveChangesAsync(ct);
        if (transaction is not null) await transaction.CommitAsync(ct);
        return await ReleaseAsync(id, ct);
    }

    public async Task DeleteArtifactAsync(Guid releaseId, Guid artifactId, string actor, CancellationToken ct)
    {
        await using var transaction = await BeginTransactionAsync(ct);
        var release = await LockReleaseAsync(releaseId, ct);
        EnsureEditable(release);
        var artifact = await db.AppArtifacts.SingleOrDefaultAsync(artifact => artifact.Id == artifactId && artifact.ReleaseId == releaseId, ct)
            ?? throw new NotFoundException("Fichier introuvable.");
        var path = StoragePath(artifact.RelativePath);
        db.AppArtifacts.Remove(artifact);
        Audit(release.AppId, release.Id, artifactId, actor, "artifact-deleted");
        if (!await db.AppArtifacts.AnyAsync(other => other.ReleaseId == releaseId && other.Id != artifactId, ct)) release.Status = "Draft";
        await db.SaveChangesAsync(ct);
        if (transaction is not null) await transaction.CommitAsync(ct);
        if (File.Exists(path)) File.Delete(path);
    }

    public async Task<(Stream Stream, StoreArtifactDto Artifact, DateTime UploadedAt)> DownloadAsync(Guid id, CancellationToken ct)
    {
        var artifact = await PublicArtifacts().SingleOrDefaultAsync(artifact => artifact.Id == id, ct)
            ?? throw new NotFoundException("Fichier introuvable.");
        var path = StoragePath(artifact.RelativePath);
        if (!File.Exists(path) || new FileInfo(path).Length != artifact.FileSize) throw new NotFoundException("Fichier introuvable.");
        return (new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, true), MapArtifact(artifact), artifact.UploadedAt);
    }

    private static void EnsureEditable(Release release)
    {
        if (release.PublishedAt is not null || release.Status is not ("Draft" or "Uploaded"))
            throw new DuplicateException("Une release publiée ou retirée est immuable.");
    }

    private string StoragePath(string relative)
    {
        var root = Path.GetFullPath(storage.Value.AppFilesPath);
        if (string.IsNullOrWhiteSpace(storage.Value.AppFilesPath) || root == Path.GetPathRoot(root) || Path.IsPathFullyQualified(relative))
            throw new InvalidOperationException("Stockage Store invalide.");
        var path = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
        var comparison = System.OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!path.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, comparison))
            throw new ValidationException("Chemin invalide.");
        for (var directory = Path.GetDirectoryName(path); directory is not null && directory.StartsWith(root, comparison); directory = Path.GetDirectoryName(directory))
            if (Directory.Exists(directory) && (File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0)
                throw new ValidationException("Lien de stockage interdit.");
        if (File.Exists(path) && (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
            throw new ValidationException("Lien de stockage interdit.");
        return path;
    }

    private static void ValidatePackage(string path, StoreArtifactRequest request)
    {
        try
        {
            using var archive = ZipFile.OpenRead(path);
            if (archive.Entries.Count is < 1 or > 10000) throw new ValidationException("Archive invalide.");
            long expandedSize = 0;
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in archive.Entries)
            {
                expandedSize = checked(expandedSize + entry.Length);
                if (expandedSize > 1_073_741_824 || entry.FullName.Length > 1024 || !names.Add(entry.FullName) ||
                    entry.FullName.Contains('\\') || entry.FullName.StartsWith('/') || entry.FullName.Split('/').Any(part => part is "." or ".."))
                    throw new ValidationException("Archive invalide.");
            }
            if (request.PackageType == "apk")
            {
                if (archive.GetEntry("AndroidManifest.xml") is null) throw new ValidationException("Manifeste Android absent.");
                // Les métadonnées Android sont déclarées par l'éditeur autorisé. Le client doit
                // vérifier le manifeste et la signature native via Android avant installation.
                return;
            }
            var manifest = archive.GetEntry("AppxManifest.xml");
            if (manifest is null || manifest.Length > 1_048_576 || archive.GetEntry("AppxBlockMap.xml") is null || archive.GetEntry("AppxSignature.p7x") is null)
                throw new ValidationException("Structure MSIX signée requise.");
            using var stream = manifest.Open();
            using var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 1_048_576 });
            var document = XDocument.Load(reader);
            var identity = document.Root?.Elements().SingleOrDefault(element => element.Name.LocalName == "Identity");
            if (identity is null || (string?)identity.Attribute("Name") != request.PackageId ||
                (string?)identity.Attribute("Version") != request.PackageVersion ||
                (string?)identity.Attribute("ProcessorArchitecture") != request.Architecture ||
                string.IsNullOrWhiteSpace((string?)identity.Attribute("Publisher")))
                throw new ValidationException("Identité MSIX différente des métadonnées déclarées.");
            var minimumVersions = document.Descendants().Where(element => element.Name.LocalName == "TargetDeviceFamily")
                .Select(element => Version.Parse((string?)element.Attribute("MinVersion") ?? "")).ToList();
            if (minimumVersions.Count == 0 || Version.Parse(request.MinimumOsVersion) < minimumVersions.Max())
                throw new ValidationException("Version minimale Windows incohérente.");
        }
        catch (Exception exception) when (exception is InvalidDataException or XmlException or FormatException or OverflowException or InvalidOperationException)
        {
            throw new ValidationException("Paquet invalide.");
        }
    }
}
