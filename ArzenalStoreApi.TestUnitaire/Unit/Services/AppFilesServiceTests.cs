using Arzenal.Dto.DTOs.FileDto;
using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Store.Api.Service.Services.AppFilesService;
using ArzenalStoreInfrastructure.Configurations;
using ArzenalStoreInfrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;
using System.Text;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Services
{
    public class AppFilesServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly AppFilesService _service;

        public AppFilesServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            // Ajout d'une app pour les tests
            var testApp = new App { Id = Guid.NewGuid(), Name = "TestApp", Version = "Alpha" };
            _context.Apps.Add(testApp);
            _context.SaveChanges();

            var storageSettings = Options.Create(new StorageSettings
            {
                AppFilesPath = Path.Combine(Directory.GetCurrentDirectory(), "TestStorage")
            });

            _service = new AppFilesService(_context, storageSettings);
        }

        private void RegisterArtifact(
            App app,
            string storagePath,
            string filePath,
            string type,
            string locator,
            string contentType,
            string? version = null,
            string? platform = null)
        {
            _context.AppArtifacts.Add(new AppArtifact
            {
                AppId = app.Id,
                Type = type,
                Locator = locator,
                Version = version,
                Platform = platform,
                OriginalFileName = Path.GetFileName(filePath),
                RelativePath = Path.GetRelativePath(storagePath, filePath),
                ContentType = contentType,
                FileSize = new FileInfo(filePath).Length
            });
            _context.SaveChanges();
        }

        [Fact]
        public async Task UploadFile_Should_Save_File_And_Update_Db()
        {
            // Arrange
            var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var deviceName = "TestDevice";

            // Crée un ClaimsPrincipal avec le claim userId
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()) // ou ClaimTypes.NameIdentifier selon ton code
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            // Crée le HttpContext
            var httpContext = new DefaultHttpContext();
            httpContext.User = principal;
            httpContext.Request.Headers["X-Device-Name"] = deviceName;

            // Mock IHttpContextAccessor
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);
            var app = await _context.Apps.FirstAsync(); // App existante en base
            var content = new byte[] { 0x4D, 0x5A, 0x00, 0x01 };

            // Création d'un stream mémoire simulant le fichier
            await using var stream = new MemoryStream(content);

            // DTO sans fichier, car le flux passe à part
            var fileDto = new UploadFileDto
            {
                AppId = app.Id,
                Version = app.Version,
                Platform = "Windows",
                Type = "app",
                FileName = "installer.exe"
            };

            // Définir un dossier temporaire pour le test
            var testStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "TestStorage");
            Directory.CreateDirectory(testStoragePath);

            var storageSettingsForTest = Options.Create(new StorageSettings
            {
                AppFilesPath = testStoragePath
            });
            var service = new AppFilesService(_context, storageSettingsForTest);

            // Act
            var savedFilePath = await service.UploadFile(fileDto, stream);

            // Assert
            Assert.NotNull(savedFilePath);

            // Vérification que le dossier existe
            var expectedDir = Path.Combine(testStoragePath, app.Id.ToString(), app.Version, "windows");
            Assert.True(Directory.Exists(expectedDir));

            // Vérification que le fichier a été créé
            var expectedFile = Path.Combine(expectedDir, "app.exe");
            Assert.True(File.Exists(expectedFile));

            // Vérification du contenu
            var fileContent = await File.ReadAllBytesAsync(expectedFile);
            Assert.Equal(content, fileContent);
            var artifact = await _context.AppArtifacts.SingleAsync();
            Assert.Equal(app.Version, artifact.Version);
            Assert.Equal("windows", artifact.Platform);
            Assert.Equal("installer.exe", artifact.OriginalFileName);

            // Cleanup après test
            Directory.Delete(testStoragePath, recursive: true);
        }

        [Fact]
        public async Task Download_FileDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            var app = await _context.Apps.FirstAsync();

            // Aucun fichier sur disque

            // Act + Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.Download(app.Id, "1.0.0", "windows")
            );

            Assert.Equal("Fichier introuvable.", ex.Message);
        }

        [Fact]
        public async Task GetAppIcon_WithPngIcon_ReturnsFileResultDtoWithCorrectContentType()
        {
            // Arrange
            var app = await _context.Apps.FirstAsync();
            var testStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "TestStorage_GetAppIcon_PNG");
            var appDir = Path.Combine(testStoragePath, app.Id.ToString());
            Directory.CreateDirectory(appDir);

            // Créer un fichier d'icône PNG
            var iconPath = Path.Combine(appDir, "icon.png");
            await File.WriteAllTextAsync(iconPath, "fake png content");
            RegisterArtifact(app, testStoragePath, iconPath, "icon", "icon", "image/png");

            var storageSettings = Options.Create(new StorageSettings
            {
                AppFilesPath = testStoragePath
            });
            var service = new AppFilesService(_context, storageSettings);

            // Act
            var result = await service.GetAppIcon(app.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("icon.png", result.FileName);
            Assert.Equal("image/png", result.ContentType);
            Assert.NotNull(result.Stream);
            Assert.True(result.Stream.CanRead);

            // Cleanup
            result.Stream.Dispose();
            Directory.Delete(testStoragePath, recursive: true);
        }

        [Fact]
        public async Task GetAppIcon_WithJpgIcon_ReturnsFileResultDtoWithCorrectContentType()
        {
            // Arrange
            var app = await _context.Apps.FirstAsync();
            var testStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "TestStorage_GetAppIcon_JPG");
            var appDir = Path.Combine(testStoragePath, app.Id.ToString());
            Directory.CreateDirectory(appDir);

            // Créer un fichier d'icône JPG
            var iconPath = Path.Combine(appDir, "icon.jpg");
            await File.WriteAllTextAsync(iconPath, "fake jpg content");
            RegisterArtifact(app, testStoragePath, iconPath, "icon", "icon", "image/jpeg");

            var storageSettings = Options.Create(new StorageSettings
            {
                AppFilesPath = testStoragePath
            });
            var service = new AppFilesService(_context, storageSettings);

            // Act
            var result = await service.GetAppIcon(app.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("icon.jpg", result.FileName);
            Assert.Equal("image/jpeg", result.ContentType);
            Assert.NotNull(result.Stream);

            // Cleanup
            result.Stream.Dispose();
            Directory.Delete(testStoragePath, recursive: true);
        }

        [Fact]
        public async Task GetAppIcon_WithJpegIcon_ReturnsFileResultDtoWithCorrectContentType()
        {
            // Arrange
            var app = await _context.Apps.FirstAsync();
            var testStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "TestStorage_GetAppIcon_JPEG");
            var appDir = Path.Combine(testStoragePath, app.Id.ToString());
            Directory.CreateDirectory(appDir);

            // Créer un fichier d'icône JPEG
            var iconPath = Path.Combine(appDir, "icon.jpeg");
            await File.WriteAllTextAsync(iconPath, "fake jpeg content");
            RegisterArtifact(app, testStoragePath, iconPath, "icon", "icon", "image/jpeg");

            var storageSettings = Options.Create(new StorageSettings
            {
                AppFilesPath = testStoragePath
            });
            var service = new AppFilesService(_context, storageSettings);

            // Act
            var result = await service.GetAppIcon(app.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("icon.jpeg", result.FileName);
            Assert.Equal("image/jpeg", result.ContentType);
            Assert.NotNull(result.Stream);

            // Cleanup
            result.Stream.Dispose();
            Directory.Delete(testStoragePath, recursive: true);
        }

        [Fact]
        public async Task GetAppIcon_WithIcoIcon_ReturnsFileResultDtoWithCorrectContentType()
        {
            // Arrange
            var app = await _context.Apps.FirstAsync();
            var testStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "TestStorage_GetAppIcon_ICO");
            var appDir = Path.Combine(testStoragePath, app.Id.ToString());
            Directory.CreateDirectory(appDir);

            // Créer un fichier d'icône ICO
            var iconPath = Path.Combine(appDir, "icon.ico");
            await File.WriteAllTextAsync(iconPath, "fake ico content");
            RegisterArtifact(app, testStoragePath, iconPath, "icon", "icon", "image/x-icon");

            var storageSettings = Options.Create(new StorageSettings
            {
                AppFilesPath = testStoragePath
            });
            var service = new AppFilesService(_context, storageSettings);

            // Act
            var result = await service.GetAppIcon(app.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("icon.ico", result.FileName);
            Assert.Equal("image/x-icon", result.ContentType);
            Assert.NotNull(result.Stream);

            // Cleanup
            result.Stream.Dispose();
            Directory.Delete(testStoragePath, recursive: true);
        }

        [Fact]
        public async Task GetAppIcon_WithUnknownExtension_ReturnsFileResultDtoWithDefaultContentType()
        {
            // Arrange
            var app = await _context.Apps.FirstAsync();
            var testStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "TestStorage_GetAppIcon_Unknown");
            var appDir = Path.Combine(testStoragePath, app.Id.ToString());
            Directory.CreateDirectory(appDir);

            // Créer un fichier d'icône avec extension inconnue
            var iconPath = Path.Combine(appDir, "icon.webp");
            await File.WriteAllTextAsync(iconPath, "fake webp content");
            RegisterArtifact(app, testStoragePath, iconPath, "icon", "icon", "image/webp");

            var storageSettings = Options.Create(new StorageSettings
            {
                AppFilesPath = testStoragePath
            });
            var service = new AppFilesService(_context, storageSettings);

            // Act
            var result = await service.GetAppIcon(app.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("icon.webp", result.FileName);
            Assert.Equal("image/webp", result.ContentType);
            Assert.NotNull(result.Stream);

            // Cleanup
            result.Stream.Dispose();
            Directory.Delete(testStoragePath, recursive: true);
        }

        [Fact]
        public async Task GetAppIcon_AppNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var nonExistentAppId = Guid.NewGuid();
            var testStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "TestStorage_GetAppIcon_NotFound");
            Directory.CreateDirectory(testStoragePath);

            var storageSettings = Options.Create(new StorageSettings
            {
                AppFilesPath = testStoragePath
            });
            var service = new AppFilesService(_context, storageSettings);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => service.GetAppIcon(nonExistentAppId));
            Assert.Equal("Aucune application trouvée.", exception.Message);

            // Cleanup
            Directory.Delete(testStoragePath, recursive: true);
        }

        [Fact]
        public async Task GetAppIcon_DirectoryDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            var app = await _context.Apps.FirstAsync();
            var testStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "TestStorage_GetAppIcon_DirNotFound");
            Directory.CreateDirectory(testStoragePath);

            // Ne pas créer le répertoire de l'app
            var storageSettings = Options.Create(new StorageSettings
            {
                AppFilesPath = testStoragePath
            });
            var service = new AppFilesService(_context, storageSettings);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => service.GetAppIcon(app.Id));
            Assert.Equal("Icône introuvable.", exception.Message);

            // Cleanup
            Directory.Delete(testStoragePath, recursive: true);
        }

        [Fact]
        public async Task GetAppIcon_NoIconFileInDirectory_ThrowsNotFoundException()
        {
            // Arrange
            var app = await _context.Apps.FirstAsync();
            var testStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "TestStorage_GetAppIcon_NoIcon");
            var appDir = Path.Combine(testStoragePath, app.Id.ToString());
            Directory.CreateDirectory(appDir);

            // Créer un fichier qui n'est pas une icône
            var filePath = Path.Combine(appDir, "readme.txt");
            await File.WriteAllTextAsync(filePath, "some content");

            var storageSettings = Options.Create(new StorageSettings
            {
                AppFilesPath = testStoragePath
            });
            var service = new AppFilesService(_context, storageSettings);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => service.GetAppIcon(app.Id));
            Assert.Equal("Icône introuvable.", exception.Message);

            // Cleanup
            Directory.Delete(testStoragePath, recursive: true);
        }

        [Fact]
        public async Task GetAppIcon_MultipleIconFiles_ReturnsFirstIcon()
        {
            // Arrange
            var app = await _context.Apps.FirstAsync();
            var testStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "TestStorage_GetAppIcon_Multiple");
            var appDir = Path.Combine(testStoragePath, app.Id.ToString());
            Directory.CreateDirectory(appDir);

            // Créer plusieurs fichiers d'icône
            var iconPath1 = Path.Combine(appDir, "icon.png");
            var iconPath2 = Path.Combine(appDir, "icon.jpg");
            await File.WriteAllTextAsync(iconPath1, "fake png content");
            await File.WriteAllTextAsync(iconPath2, "fake jpg content");
            RegisterArtifact(app, testStoragePath, iconPath1, "icon", "icon", "image/png");

            var storageSettings = Options.Create(new StorageSettings
            {
                AppFilesPath = testStoragePath
            });
            var service = new AppFilesService(_context, storageSettings);

            // Act
            var result = await service.GetAppIcon(app.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("icon.png", result.FileName);
            Assert.NotNull(result.Stream);

            // Cleanup
            result.Stream.Dispose();
            Directory.Delete(testStoragePath, recursive: true);
        }

        [Fact]
        public async Task Download_FileExists_ReturnsAppFileStreamResult()
        {
            // Arrange
            var app = await _context.Apps.FirstAsync();

            // Crée un fichier réel
            var testStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "TestStorage");
            var appDir = Path.Combine(testStoragePath, app.Id.ToString(), "1.0.0", "windows");
            Directory.CreateDirectory(appDir);

            var filePath = Path.Combine(appDir, "app.exe");
            await File.WriteAllBytesAsync(filePath, new byte[] { 1, 2, 3 });
            RegisterArtifact(app, testStoragePath, filePath, "app", "1.0.0|windows", "application/octet-stream", "1.0.0", "windows");

            // Act
            var result = await _service.Download(app.Id, "1.0.0", "windows");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("app.exe", result.FileName);
            Assert.Equal("application/octet-stream", result.ContentType);

            using var ms = new MemoryStream();
            await result.Stream.CopyToAsync(ms);
            Assert.Equal(new byte[] { 1, 2, 3 }, ms.ToArray());

            result.Stream.Dispose();
        }

        [Fact]
        public async Task Download_UsesTheRequestedVersionAndPlatform_NotTheMostRecentFile()
        {
            var app = await _context.Apps.FirstAsync();
            var testStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "TestStorage");
            var v1Path = Path.Combine(testStoragePath, app.Id.ToString(), "1.0.0", "windows", "app.exe");
            var v2Path = Path.Combine(testStoragePath, app.Id.ToString(), "2.0.0", "windows", "app.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(v1Path)!);
            Directory.CreateDirectory(Path.GetDirectoryName(v2Path)!);
            await File.WriteAllBytesAsync(v1Path, [1]);
            await File.WriteAllBytesAsync(v2Path, [2]);
            RegisterArtifact(app, testStoragePath, v1Path, "app", "1.0.0|windows", "application/octet-stream", "1.0.0", "windows");
            RegisterArtifact(app, testStoragePath, v2Path, "app", "2.0.0|windows", "application/octet-stream", "2.0.0", "windows");

            var result = await _service.Download(app.Id, "1.0.0", "windows");

            await using var stream = result.Stream;
            using var content = new MemoryStream();
            await stream.CopyToAsync(content);
            Assert.Equal([1], content.ToArray());
        }


        [Fact]
        public async Task DeleteFile_Should_Remove_Folder()
        {
            // Arrange
            var app = await _context.Apps.FirstAsync();
            var appDir = Path.Combine(Directory.GetCurrentDirectory(), "TestStorage", app.Id.ToString());
            Directory.CreateDirectory(appDir);

            // Act
            var result = await _service.DeleteFile(app.Id);

            // Assert
            Assert.True(result);
            Assert.False(Directory.Exists(appDir));
        }

        [Fact]
        public async Task GetFilePath_Should_Return_Correct_Path()
        {
            // Arrange
            var app = await _context.Apps.FirstAsync();
            var appDir = Path.Combine(Directory.GetCurrentDirectory(), "TestStorage", app.Id.ToString());
            Directory.CreateDirectory(appDir);
            var filePath = Path.Combine(appDir);

            // Act
            var path = await _service.GetFilePath(app.Id);

            // Assert
            Assert.Equal(filePath, path);
        }
    }
}
