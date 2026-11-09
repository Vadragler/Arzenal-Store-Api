using System.IO.Compression;
using System.Security.Cryptography;
using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.Service.Contracts.Store;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Store.Api.Service.Services.Store;
using ArzenalStoreInfrastructure.Configurations;
using ArzenalStoreInfrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Services;

public class StoreUploadServiceTests
{
    [Theory]
    [InlineData("size")]
    [InlineData("scanner")]
    [InlineData("archive")]
    [InlineData("cancel")]
    public async Task FailedUpload_DoesNotLeaveAFileOrArtifact(string failure)
    {
        using var key = RSA.Create(3072);
        var directory = Path.Combine(Path.GetTempPath(), "ArzenalStoreUploadTests", Guid.NewGuid().ToString("N"));
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new ApplicationDbContext(options);
        var app = new App { Name = "Upload test", Slug = "upload-test", Version = "1.0.0" };
        var release = new Release { AppId = app.Id, Status = "Draft", Version = "1.0.0" };
        db.Apps.Add(app);
        db.Releases.Add(release);
        await db.SaveChangesAsync();
        var bytes = Apk(failure == "archive");
        var signature = Convert.ToBase64String(key.SignData(bytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pss));
        var scanner = new Mock<IFileScanner>();
        scanner.Setup(service => service.ScanAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Malware détecté."));
        var settings = Options.Create(new StoreDistributionSettings { PublisherPublicKeys = new() { ["test"] = key.ExportSubjectPublicKeyInfoPem() } });
        var service = new StoreService(db, Options.Create(new StorageSettings
        {
            AppFilesPath = directory,
            MaxUploadBytes = failure == "size" ? 1 : 1024 * 1024,
            RequireMalwareScan = failure == "scanner"
        }), settings, new PublisherSignatureVerifier(settings), scanner.Object);
        var request = new StoreArtifactRequest
        {
            Platform = "android",
            Architecture = "arm64",
            PackageType = "apk",
            PackageId = "ovh.arzenal.test",
            PackageVersion = "1.0.0",
            MinimumOsVersion = "26",
            AndroidVersionCode = 100
        };
        try
        {
            using var input = new MemoryStream(bytes);
            var ct = new CancellationToken(failure == "cancel");
            if (failure == "cancel")
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.UploadAsync(release.Id, request, input, "test", signature, "test-actor", ct));
            else
                await Assert.ThrowsAsync<ValidationException>(() => service.UploadAsync(release.Id, request, input, "test", signature, "test-actor", ct));
            Assert.Empty(await db.AppArtifacts.ToListAsync());
            Assert.Equal("Draft", release.Status);
            Assert.Empty(await db.PublicationAudits.ToListAsync());
            if (Directory.Exists(directory)) Assert.Empty(Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories));
            if (failure == "scanner") scanner.Verify(item => item.ScanAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    private static byte[] Apk(bool unsafePath)
    {
        using var output = new MemoryStream();
        using (var zip = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            using (var writer = new StreamWriter(zip.CreateEntry("AndroidManifest.xml").Open())) writer.Write("inert fixture");
            if (unsafePath)
            {
                using var writer = new StreamWriter(zip.CreateEntry("../escape").Open());
                writer.Write("rejected");
            }
        }
        return output.ToArray();
    }
}
