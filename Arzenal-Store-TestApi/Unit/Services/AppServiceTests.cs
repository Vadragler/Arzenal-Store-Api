using ArzenalStoreApi.Data;
using ArzenalStoreApi.Exceptions;
using ArzenalStoreApi.Models;
using ArzenalStoreApi.Services.AppService;
using ArzenalStoreSharedDto.DTOs.AppDto;
using Microsoft.EntityFrameworkCore;
using Xunit;
using OperatingSystem = ArzenalStoreApi.Models.OperatingSystem;

namespace TestArzenalStoreApi.Unit.Services
{
    public class AppServiceTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // base unique pour chaque test
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetAllAppsAsync_ReturnsApps_WhenAppsExist()
        {
            // Arrange
            using var db = GetDbContext();
            var service = new AppService(db);

            db.Categories.Add(new Categorie { Id = 1, Name = "Cat" });
            db.Apps.Add(new App
            {
                Id = Guid.NewGuid(),
                Name = "TestApp",
                Version = "1.0",
                FilePath = "test.exe",
                IsVisible = true,
                CategoryId = 1,
                AppSize = 123,
                ReleaseDate = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            // Act
            var result = await service.GetAllAppsAsync();

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal("TestApp", result[0].Name);
        }

        [Fact]
        public async Task GetAllAppsAsync_ThrowsNotFoundException_WhenNoApps()
        {
            using var db = GetDbContext();
            var service = new AppService(db);

            await Assert.ThrowsAsync<NotFoundException>(() => service.GetAllAppsAsync());
        }

        [Fact]
        public async Task GetAppByIdAsync_ReturnsApp_WhenAppExists()
        {
            using var db = GetDbContext();
            var service = new AppService(db);

            db.Categories.Add(new Categorie { Id = 1, Name = "Cat" });
            var app = new App
            {
                Id = Guid.NewGuid(),
                Name = "TestApp",
                Version = "1.0",
                FilePath = "test.exe",
                IsVisible = true,
                CategoryId = 1,
                AppSize = 123,
                ReleaseDate = DateTime.UtcNow
            };
            db.Apps.Add(app);
            await db.SaveChangesAsync();

            var result = await service.GetAppByIdAsync(app.Id);

            Assert.NotNull(result);
            Assert.Equal(app.Name, result.Name);
        }

        [Fact]
        public async Task GetAppByIdAsync_ThrowsNotFoundException_WhenAppDoesNotExist()
        {
            using var db = GetDbContext();
            var service = new AppService(db);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.GetAppByIdAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateAppAsync_CreatesApp_WhenValid()
        {
            using var db = GetDbContext();
            var service = new AppService(db);

            db.Categories.Add(new Categorie { Id = 1, Name = "Cat" });
            await db.SaveChangesAsync();

            var dto = new CreateAppDto
            {
                Name = "NewApp",
                Version = "1.0",
                FilePath = "new.exe",
                IsVisible = true,
                CategoryId = 1,
                LastUpdated = DateTime.UtcNow,
                AppSize = 100
            };

            var result = await service.CreateAppAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
        }

        [Theory]
        [InlineData("Duplicate")]
        [InlineData("InvalidCategory")]
        [InlineData("InvalidLanguage")]
        [InlineData("InvalidTag")]
        [InlineData("InvalidOS")]
        public async Task CreateAppAsync_ThrowsValidation_WhenInputInvalid(string scenario)
        {
            using var db = GetDbContext();
            var service = new AppService(db);

            // Ajouter une catégorie, langue, tag et OS valides
            db.Categories.Add(new Categorie { Id = 1, Name = "Cat" });
            db.Languages.Add(new Language { Id = 1, Name = "Lang" });
            db.Tags.Add(new Tag { Id = 1, Name = "Tag" });
            db.OperatingSystems.Add(new OperatingSystem { Id = 1, Name = "OS" });
            await db.SaveChangesAsync();

            var dto = new CreateAppDto
            {
                Name = "TestApp",
                Version = "1.0",
                FilePath = "file.exe",
                IsVisible = true,
                CategoryId = 1,
                LanguageIds = new List<int> { 1 },
                TagIds = new List<int> { 1 },
                OsIds = new List<int> { 1 },
                LastUpdated = DateTime.UtcNow,
                AppSize = 100
            };

            if (scenario == "Duplicate")
            {
                db.Apps.Add(new App { Id = Guid.NewGuid(), Name = "TestApp", CategoryId = 1,Version = "1.0",FilePath = "file.exe" });
                await db.SaveChangesAsync();
                await Assert.ThrowsAsync<DuplicateException>(() => service.CreateAppAsync(dto));
            }
            else if (scenario == "InvalidCategory")
            {
                dto.CategoryId = 999;
                await Assert.ThrowsAsync<ValidationException>(() => service.CreateAppAsync(dto));
            }
            else if (scenario == "InvalidLanguage")
            {
                dto.LanguageIds = new List<int> { 999 };
                await Assert.ThrowsAsync<ValidationException>(() => service.CreateAppAsync(dto));
            }
            else if (scenario == "InvalidTag")
            {
                dto.TagIds = new List<int> { 999 };
                await Assert.ThrowsAsync<ValidationException>(() => service.CreateAppAsync(dto));
            }
            else if (scenario == "InvalidOS")
            {
                dto.OsIds = new List<int> { 999 };
                await Assert.ThrowsAsync<ValidationException>(() => service.CreateAppAsync(dto));
            }
        }


       

        [Fact]
        public async Task UpdateAppAsync_UpdatesApp_WhenValid()
        {
            using var db = GetDbContext();
            var service = new AppService(db);

            db.Categories.Add(new Categorie { Id = 1, Name = "Cat" });
            var app = new App
            {
                Id = Guid.NewGuid(),
                Name = "ToUpdate",
                Version = "1.0",
                FilePath = "update.exe",
                IsVisible = true,
                CategoryId = 1,
                AppSize = 123,
                ReleaseDate = DateTime.UtcNow
            };
            db.Apps.Add(app);
            await db.SaveChangesAsync();

            var dto = new UpdateAppDto { Name = "UpdatedName" };
            var result = await service.UpdateAppAsync(app.Id, dto);

            Assert.NotNull(result);
            Assert.Equal("UpdatedName", result.Name);
        }

        [Fact]
        public async Task UpdateAppAsync_ThrowsNotFoundException_WhenAppDoesNotExist()
        {
            using var db = GetDbContext();
            var service = new AppService(db);

            var dto = new UpdateAppDto { Name = "NewName" };
            await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateAppAsync(Guid.NewGuid(), dto));
        }


        [Fact]
        public async Task UpdateAppAsync_ThrowsDuplicateException_WhenNameAlreadyExists()
        {
            using var db = GetDbContext();
            var service = new AppService(db);

            db.Apps.AddRange(
                new App { Id = Guid.NewGuid(), Name = "Existing", Version = "1.0", FilePath = "file.exe" },
                new App { Id = Guid.NewGuid(), Name = "ToUpdate", Version = "1.0", FilePath = "file.exe" }
            );
            await db.SaveChangesAsync();

            var appToUpdate = await db.Apps.FirstAsync(a => a.Name == "ToUpdate");
            var dto = new UpdateAppDto { Name = "Existing" };

            await Assert.ThrowsAsync<DuplicateException>(() => service.UpdateAppAsync(appToUpdate.Id, dto));
        }

        [Fact]
        public async Task UpdateAppAsync_ThrowsValidationException_WhenCategoryInvalid()
        {
            using var db = GetDbContext();
            var service = new AppService(db);

            var app = new App { Id = Guid.NewGuid(), Name = "App1", CategoryId = 1, Version = "1.0", FilePath = "file.exe" };
            db.Apps.Add(app);
            await db.SaveChangesAsync();

            var dto = new UpdateAppDto { CategoryId = 999 };
            await Assert.ThrowsAsync<ValidationException>(() => service.UpdateAppAsync(app.Id, dto));
        }



        [Theory]
        [InlineData("Languages")]
        [InlineData("Tags")]
        [InlineData("OS")]
        public async Task UpdateAppAsync_ThrowsValidationException_ForInvalidCollections(string type)
        {
            using var db = GetDbContext();
            var service = new AppService(db);

            var app = new App { Id = Guid.NewGuid(), Name = "App1", CategoryId = 1, Version = "1.0", FilePath = "file.exe" };
            db.Apps.Add(app);
            await db.SaveChangesAsync();

            var dto = new UpdateAppDto();
            switch (type)
            {
                case "Languages": dto.LanguageIds = new List<int> { 999 }; break;
                case "Tags": dto.TagIds = new List<int> { 999 }; break;
                case "OS": dto.OsIds = new List<int> { 999 }; break;
            }

            await Assert.ThrowsAsync<ValidationException>(() => service.UpdateAppAsync(app.Id, dto));
        }




        [Fact]
        public async Task DeleteAppAsync_DeletesApp_WhenExists()
        {
            using var db = GetDbContext();
            var service = new AppService(db);

            db.Categories.Add(new Categorie { Id = 1, Name = "Cat" });
            var app = new App
            {
                Id = Guid.NewGuid(),
                Name = "ToDelete",
                Version = "1.0",
                FilePath = "del.exe",
                IsVisible = true,
                CategoryId = 1,
                AppSize = 123,
                ReleaseDate = DateTime.UtcNow
            };
            db.Apps.Add(app);
            await db.SaveChangesAsync();

            var result = await service.DeleteAppAsync(app.Id);

            Assert.True(result);
            Assert.Null(await db.Apps.FindAsync(app.Id));
        }
    }
}
