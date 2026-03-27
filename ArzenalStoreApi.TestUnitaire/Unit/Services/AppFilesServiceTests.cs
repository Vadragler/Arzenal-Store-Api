using Arzenal.Dto.DTOs.FileDto;
using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Services.AppFilesService;
using ArzenalStoreInfrastructure.Configurations;
using ArzenalStoreInfrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            _context = new ApplicationDbContext(options);

            // Ajout d'une app pour les tests
            var testApp = new App { Id = Guid.NewGuid(), Name = "TestApp", Version="Alpha" };
            _context.Apps.Add(testApp);
            _context.SaveChanges();

            var storageSettings = Options.Create(new StorageSettings
            {
                AppFilesPath = Path.Combine(Directory.GetCurrentDirectory(), "TestStorage")
            });

            _service = new AppFilesService(_context, storageSettings);
        }

        [Fact]
        public async Task UploadFile_Should_Save_File_And_Update_Db()
        {
            // Arrange
            var app = await _context.Apps.FirstAsync(); // App existante en base
            var content = "Hello World";
            var fileName = "app.txt"; // juste pour extension / test

            // Création d'un stream mémoire simulant le fichier
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            // DTO sans fichier, car le flux passe à part
            var fileDto = new UploadFileDto
            {
                AppId = app.Id,
                Version = app.Version,
                Platform = "Windows",
                Type = "app",
                FileName = "app.txt"
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
            var expectedDir = Path.Combine(testStoragePath, app.Id.ToString(), app.Version, "Windows");
            Assert.True(Directory.Exists(expectedDir));

            // Vérification que le fichier a été créé
            var expectedFile = Path.Combine(expectedDir, "app.txt");
            Assert.True(File.Exists(expectedFile));

            // Vérification du contenu
            var fileContent = await File.ReadAllTextAsync(expectedFile);
            Assert.Equal(content, fileContent);

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
                _service.Download(app.Id)
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
            Assert.Equal("application/octet-stream", result.ContentType);
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
            Assert.Equal("Fichier introuvable.", exception.Message);

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

            var storageSettings = Options.Create(new StorageSettings
            {
                AppFilesPath = testStoragePath
            });
            var service = new AppFilesService(_context, storageSettings);

            // Act
            var result = await service.GetAppIcon(app.Id);

            // Assert
            Assert.NotNull(result);
            // Le résultat doit être l'un des deux fichiers existants
            Assert.True(result.FileName == "icon.png" || result.FileName == "icon.jpg");
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
            var appDir = Path.Combine(testStoragePath, app.Id.ToString());
            Directory.CreateDirectory(appDir);

            var filePath = Path.Combine(appDir, "app.exe");
            await File.WriteAllBytesAsync(filePath, new byte[] { 1, 2, 3 });

            // Act
            var result = await _service.Download(app.Id);

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
