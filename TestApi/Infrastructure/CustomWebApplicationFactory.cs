using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.AspNetCore.Hosting;
using ArzenalApi.Data;
using ArzenalApi.Models;
//using OperatingSystem = ArzenalApi.Models.OperatingSystem;
using Microsoft.Extensions.Logging;


namespace TestApi.Infrastructure
{
    using Microsoft.Data.Sqlite; // pour SqliteConnection
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using System;
    using System.Linq;
    using Microsoft.Extensions.Hosting;
    using Microsoft.AspNetCore.Authentication;

    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private static SqliteConnection _connection; // devient static
        private static bool _databaseInitialized;    // pour ne créer la base qu'une seule fois

        public IServiceProvider Services => Server?.Services;
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remplace le schéma d'authentification par notre "Fake"
                services.AddAuthentication("Fake")
                        .AddScheme<AuthenticationSchemeOptions, FakeJwtAuthHandler>(
                            "Fake", options => { });
            });

            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Tu peux modifier ici la config si nécessaire
            });

            return base.CreateHost(builder);
        }
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            SQLitePCL.Batteries_V2.Init();

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Connexion unique
                if (_connection == null)
                {
                    _connection = new SqliteConnection("Filename=:memory:");
                    _connection.Open();
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();

                if (!_databaseInitialized)
                {
                    db.Database.EnsureCreated();
                    SeedDatabase(db);

                    _databaseInitialized = true;
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            // On ne ferme pas la connexion ici pour garder la base entre tests
            // => tu peux ajouter une méthode spéciale si tu veux la fermer à la fin de toute la campagne de tests
        }

        private static void SeedDatabase(ApplicationDbContext context)
        {
            // ==> Ton code de seed EXACT sans rien changer
            var language1 = new Language { Id = 1, Name = "English" };
            var language2 = new Language { Id = 2, Name = "French" };
            var tag1 = new Tag { Id = 1, Name = "Productivity" };
            var tag2 = new Tag { Id = 2, Name = "Utility" };
            var os1 = new ArzenalApi.Models.OperatingSystem { Id = 1, Name = "Windows" };
            var os2 = new ArzenalApi.Models.OperatingSystem { Id = 2, Name = "Linux" };
            var category = new Categorie { Id = 1, Name = "Software" };

            context.Languages.AddRange(language1, language2);
            context.Tags.AddRange(tag1, tag2);
            context.OperatingSystems.AddRange(os1, os2);
            context.Categories.Add(category);

            context.SaveChanges();

            var appId1 = Guid.NewGuid();
            var appId2 = Guid.NewGuid();

            context.Apps.AddRange(
                new App
                {
                    Id = appId1,
                    Name = "Test App 1",
                    Description = "First test application",
                    Requirements = "Windows 10 or later",
                    Version = "1.0.0",
                    FilePath = "C:/Test_App_1.exe",
                    Icone = "C:/Icons/test_app1.png",
                    AppSize = 150,
                    ReleaseDate = DateTime.UtcNow.AddMonths(-3),
                    LastUpdated = DateTime.UtcNow,
                    IsVisible = true,
                    CategoryId = category.Id,
                    AppLanguages = new List<AppLanguage> { new AppLanguage { AppId = appId1, LanguageId = language1.Id } },
                    AppTags = new List<AppTag> { new AppTag { AppId = appId1, TagId = tag1.Id } },
                    AppOperatingSystems = new List<AppOperatingSystem> { new AppOperatingSystem { AppId = appId1, OSId = os1.Id } }
                },
                new App
                {
                    Id = appId2,
                    Name = "Test App 2",
                    Description = "Second test application",
                    Requirements = "Linux Ubuntu 20.04+",
                    Version = "2.0.0",
                    FilePath = "C:/Test_App_2.bin",
                    Icone = "C:/Icons/test_app2.png",
                    AppSize = 250,
                    ReleaseDate = DateTime.UtcNow.AddMonths(-1),
                    LastUpdated = DateTime.UtcNow,
                    IsVisible = true,
                    CategoryId = category.Id,
                    AppLanguages = new List<AppLanguage> { new AppLanguage { AppId = appId2, LanguageId = language2.Id } },
                    AppTags = new List<AppTag> { new AppTag { AppId = appId2, TagId = tag2.Id } },
                    AppOperatingSystems = new List<AppOperatingSystem> { new AppOperatingSystem { AppId = appId2, OSId = os2.Id } }
                }
            );

            context.SaveChanges();
        }
    }

    public class AlternateCustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private SqliteConnection _connection;
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remplace le schéma d'authentification par notre "Fake"
                services.AddAuthentication("Fake")
                        .AddScheme<AuthenticationSchemeOptions, FakeJwtAuthHandler>(
                            "Fake", options => { });
            });

            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Tu peux modifier ici la config si nécessaire
            });

            return base.CreateHost(builder);
        }
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Supprimer la configuration existante
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Créer une nouvelle connexion SQLite InMemory
                _connection = new SqliteConnection("Filename=:memory:");
                _connection.Open();

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(_connection));

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                db.Database.EnsureCreated();
                // Pas de Seed ici volontairement
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }
    }


}



