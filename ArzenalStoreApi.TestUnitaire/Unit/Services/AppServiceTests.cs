using Arzenal.Store.Api.Domain.Models;
using ArzenalStoreInfrastructure.Data;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Dto.DTOs.AppDto;
using Microsoft.EntityFrameworkCore;
using OperatingSystem = Arzenal.Store.Api.Domain.Models.OperatingSystem;
using Arzenal.Store.Api.TestIntegration.Infrastructure;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Services
{
    public class AppServiceTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetAllAppsAsync_ReturnsApps_WhenAppsExist()
        {
            // Arrange
            using var db = GetDbContext();
            

            db.Categories.Add(new Categorie { Id = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"), Name = "Cat" });
            db.Apps.Add(new App
            {
                Id = Guid.NewGuid(),
                Name = "TestApp",
                Version = "1.0",
                IsVisible = true,
                CategoryId = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"),
                AppSize = 123,
                ReleaseDate = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var service = TestHelpers.CreateAppService(db);
            // Act
            var result = await service.GetAllAppsAsync();

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal("TestApp", result[0].Name);
        }

        [Fact]
        public async Task GetAllAppsAsync_ReturnMessage_WhenNoApps()
        {
            // Arrange
            using var db = GetDbContext();
            var service = TestHelpers.CreateAppService(db);

            // Act
            var result = await service.GetAllAppsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAppByIdAsync_ReturnsApp_WhenAppExists()
        {
            // Arrange
            using var db = GetDbContext();
            var service = TestHelpers.CreateAppService(db);

            db.Categories.Add(new Categorie { Id = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"), Name = "Cat" });
            var app = new App
            {
                Id = Guid.NewGuid(),
                Name = "TestApp",
                Version = "1.0",
                IsVisible = true,
                CategoryId = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"),
                AppSize = 123,
                ReleaseDate = DateTime.UtcNow
            };
            db.Apps.Add(app);
            await db.SaveChangesAsync();

            // Act
            var result = await service.GetAppByIdAsync(app.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(app.Name, result.Name);
        }

        [Fact]
        public async Task GetAppByIdAsync_ThrowsNotFoundException_WhenAppDoesNotExist()
        {
            // Arrange
            using var db = GetDbContext();
            var service = TestHelpers.CreateAppService(db);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.GetAppByIdAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateAppAsync_CreatesApp_WhenValid()
        {
            // Arrange
            using var db = GetDbContext();
            var service = TestHelpers.CreateAppService(db);

            db.Categories.Add(new Categorie { Id = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"), Name = "Cat" });
            await db.SaveChangesAsync();

            var dto = new CreateAppDto
            {
                Name = "NewApp",
                Version = "1.0",
                IsVisible = true,
                CategoryId = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"),
                LastUpdated = DateTime.UtcNow,
                AppSize = 100
            };

            // Act
            var result = await service.CreateAppAsync(dto);

            // Assert
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
            // Arrange
            using var db = GetDbContext();
            var service = TestHelpers.CreateAppService(db);
            db.Categories.Add(new Categorie { Id = Guid.NewGuid(), Name = "Cat" });
            db.Languages.Add(new Language { Id = Guid.NewGuid(), Name = "Lang" });
            db.Tags.Add(new Tag { Id = Guid.NewGuid(), Name = "Tag" });
            db.OperatingSystems.Add(new OperatingSystem { Id = Guid.NewGuid(), Name = "OS" });
            await db.SaveChangesAsync();

            var dto = new CreateAppDto
            {
                Name = "TestApp",
                Version = "1.0",
                IsVisible = true,
                CategoryId = Guid.NewGuid(),
                LanguageIds = new List<Guid> { Guid.NewGuid() },
                TagIds = new List<Guid> { Guid.NewGuid() },
                OsIds = new List<Guid> { Guid.NewGuid() },
                LastUpdated = DateTime.UtcNow,
                AppSize = 100
            };


            // Act & Assert
            if (scenario == "Duplicate")
            {
                db.Apps.Add(new App { Id = Guid.NewGuid(), Name = "TestApp", CategoryId = Guid.NewGuid(),Version = "1.0"});
                await db.SaveChangesAsync();
                await Assert.ThrowsAsync<DuplicateException>(() => service.CreateAppAsync(dto));
            } else if (scenario == "InvalidCategory")
            {
                await Assert.ThrowsAsync<ValidationException>(() => service.CreateAppAsync(dto));
            }
            else if (scenario == "InvalidLanguage")
            {
                dto.CategoryId = db.Categories.First().Id;
                await Assert.ThrowsAsync<ValidationException>(() => service.CreateAppAsync(dto));
            }
            else if (scenario == "InvalidTag")
            {
                dto.LanguageIds = new List<Guid> { db.Languages.First().Id };
                await Assert.ThrowsAsync<ValidationException>(() => service.CreateAppAsync(dto));
            }
            else if (scenario == "InvalidOS")
            {
                dto.TagIds = new List<Guid> { db.Tags.First().Id };
                await Assert.ThrowsAsync<ValidationException>(() => service.CreateAppAsync(dto));
            }
        }

        [Fact]
        public async Task UpdateAppAsync_UpdatesApp_WhenValid()
        {
            // Arrange
            using var db = GetDbContext();
            var service = TestHelpers.CreateAppService(db);

            db.Categories.Add(new Categorie { Id = new Guid(), Name = "Cat" });
            var app = new App
            {
                Id = Guid.NewGuid(),
                Name = "ToUpdate",
                Version = "1.0",
                IsVisible = true,
                CategoryId = Guid.NewGuid(),
                AppSize = 123,
                ReleaseDate = DateTime.UtcNow
            };
            db.Apps.Add(app);
            await db.SaveChangesAsync();

            var dto = new UpdateAppDto { Name = "UpdatedName" };

            // Act
            var result = await service.UpdateAppAsync(app.Id, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("UpdatedName", result.Name);
        }

        [Fact]
        public async Task UpdateAppAsync_ThrowsNotFoundException_WhenAppDoesNotExist()
        {
            // Arrange
            using var db = GetDbContext();
            var service = TestHelpers.CreateAppService(db);
            var dto = new UpdateAppDto { Name = "NewName" };

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateAppAsync(Guid.NewGuid(), dto));
        }

        [Fact]
        public async Task UpdateAppAsync_ThrowsDuplicateException_WhenNameAlreadyExists()
        {
            // Arrange
            using var db = GetDbContext();
            var service = TestHelpers.CreateAppService(db);

            db.Apps.AddRange(
                new App { Id = Guid.NewGuid(), Name = "Existing", Version = "1.0" },
                new App { Id = Guid.NewGuid(), Name = "ToUpdate", Version = "1.0" }
            );
            await db.SaveChangesAsync();
            var dto = new UpdateAppDto { Name = "Existing" };

            // Act & Assert
            var appToUpdate = await db.Apps.FirstAsync(a => a.Name == "ToUpdate");
            await Assert.ThrowsAsync<DuplicateException>(() => service.UpdateAppAsync(appToUpdate.Id, dto));
        }

        [Fact]
        public async Task DeleteAppAsync_DeletesApp_WhenExists()
        {
            // Arrange
            using var db = GetDbContext();
            var service = TestHelpers.CreateAppService(db);

            db.Categories.Add(new Categorie { Id = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"), Name = "Cat" });
            var app = new App
            {
                Id = Guid.NewGuid(),
                Name = "ToDelete",
                Version = "1.0",
                IsVisible = true,
                CategoryId = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"),
                AppSize = 123,
                ReleaseDate = DateTime.UtcNow
            };
            db.Apps.Add(app);
            await db.SaveChangesAsync();

            // Act & Assert
            var result = await service.DeleteAppAsync(app.Id);
            Assert.True(result);
            Assert.Null(await db.Apps.FindAsync(app.Id));
        }
    }
}
