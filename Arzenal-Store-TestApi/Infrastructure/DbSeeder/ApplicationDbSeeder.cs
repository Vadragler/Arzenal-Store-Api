using Arzenal.Store.Api.Domain.Models;
using ArzenalStoreInfrastructure.Data;
using OperatingSystem = Arzenal.Store.Api.Domain.Models.OperatingSystem;

namespace Arzenal.Store.Api.TestIntegration.Infrastructure.DbSeeder
{
    public static class ApplicationDbSeeder
    {
        public static void Seed(ApplicationDbContext db)
        {
            var Guid1 = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d");
            var Guid2 = Guid.Parse("f1c9b8d4-2e6a-4f3b-9a7c-5e8d2f1b3c4a");

            var language1 = new Language { Id = Guid1, Name = "English" };
            var language2 = new Language { Id = Guid2, Name = "French" };
            var tag1 = new Tag { Id = Guid1, Name = "Productivity" };
            var tag2 = new Tag { Id = Guid2, Name = "Utility" };
            var os1 = new OperatingSystem { Id = Guid1, Name = "Windows" };
            var os2 = new OperatingSystem { Id = Guid2, Name = "Linux" };
            var category = new Categorie { Id = Guid1, Name = "Software" };

            db.Languages.AddRange(language1, language2);
            db.Tags.AddRange(tag1, tag2);
            db.OperatingSystems.AddRange(os1, os2);
            db.Categories.Add(category);
            db.SaveChanges();

            var appId1 = Guid.NewGuid();
            var appId2 = Guid.NewGuid();
            var now = DateTime.UtcNow;

            db.Apps.AddRange(
                new App
                {
                    Id = Guid1,
                    Name = "Test App 1",
                    Description = "First test application",
                    Requirements = "Windows 10 or later",
                    Version = "1.0.0",
                    Icone = "C:/Icons/test_app1.png",
                    AppSize = 150,
                    ReleaseDate = now.AddMonths(-3),
                    LastUpdated = now,
                    IsVisible = true,
                    CategoryId = category.Id,
                    AppLanguages = new List<AppLanguage> { new AppLanguage { AppId = appId1, LanguageId = language1.Id } },
                    AppTags = new List<AppTag> { new AppTag { AppId = appId1, TagId = tag1.Id } },
                    AppOperatingSystems = new List<AppOperatingSystem> { new AppOperatingSystem { AppId = appId1, OSId = os1.Id } }
                },
                new App
                {
                    Id = Guid2,
                    Name = "Test App 2",
                    Description = "Second test application",
                    Requirements = "Linux Ubuntu 20.04+",
                    Version = "2.0.0",
                    Icone = "C:/Icons/test_app2.png",
                    AppSize = 250,
                    ReleaseDate = now.AddMonths(-1),
                    LastUpdated = now,
                    IsVisible = true,
                    CategoryId = category.Id,
                    AppLanguages = new List<AppLanguage> { new AppLanguage { AppId = appId2, LanguageId = language2.Id } },
                    AppTags = new List<AppTag> { new AppTag { AppId = appId2, TagId = tag2.Id } },
                    AppOperatingSystems = new List<AppOperatingSystem> { new AppOperatingSystem { AppId = appId2, OSId = os2.Id } }
                }
            );

            db.SaveChanges();
        }
    }
}
