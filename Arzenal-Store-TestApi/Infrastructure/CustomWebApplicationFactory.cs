using Arzenal.Dto.DTOs.AuthDto;
using Arzenal.Store.Api.Domain.Models.Requests;
using Arzenal.Store.Api.Infrastructure.Data;
using Arzenal.Store.Api.TestIntegration.Infrastructure.DbSeeder;
using ArzenalStoreInfrastructure.Configurations;
using ArzenalStoreInfrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace Arzenal.Store.Api.TestIntegration.Infrastructure
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private string? _dbName;
        private string? _connectionString;
        private string? _dbAuthName;
        private string? _connectionAuthString;
        public string StoragePath { get; private set; } = Path.Combine(Path.GetTempPath(), "ArzenalTest", Guid.NewGuid().ToString());

        public new IServiceProvider Services => Server.Services;

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            return base.CreateHost(builder);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Supprime DbContext existant
                var descriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                var descriptorAuth = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AuthDbContext>));
                if (descriptorAuth != null) services.Remove(descriptorAuth);

                services.Configure<StorageSettings>(opt =>
                {
                    opt.AppFilesPath = StoragePath;
                    Directory.CreateDirectory(StoragePath);
                });

                MapperMocksFactory.SetupDefaultMapperMocks(services);



                // 🔹 Création d'une base unique
                // Use shorter names to avoid exceeding MySQL lock name length (max 64 chars)
                var shortSuffix = Guid.NewGuid().ToString("N").Substring(0, 16);
                _dbName = $"arzenal_test_{shortSuffix}";
                _connectionString = $"Server=localhost;Port=3306;Database={_dbName};User=root;Password=!PKdM?MJR9wEfQa;";

                _dbAuthName = $"arzenal_auth_test_{shortSuffix}";
                _connectionAuthString = $"Server=localhost;Port=3306;Database={_dbAuthName};User=root;Password=!PKdM?MJR9wEfQa;";

                // Créer la base
                using var masterConn = new MySqlConnection("Server=localhost;Port=3306;User=root;Password=!PKdM?MJR9wEfQa;");
                masterConn.Open();
                using var createCmd = new MySqlCommand($"CREATE DATABASE `{_dbName}`;", masterConn);
                using var createAuthCmd = new MySqlCommand($"CREATE DATABASE `{_dbAuthName}`;", masterConn);
                createAuthCmd.ExecuteNonQuery();
                createCmd.ExecuteNonQuery();

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseMySql(_connectionString, new MySqlServerVersion(new Version(8,0,33))));
                services.AddDbContext<AuthDbContext>(options =>
                    options.UseMySql(_connectionAuthString, new MySqlServerVersion(new Version(8,0,33))));

                // Seed DB
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var appDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
                appDb.Database.Migrate();
                authDb.Database.Migrate();

                ApplicationDbSeeder.Seed(appDb);
                AuthDbSeeder.Seed(authDb);
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            // Supprimer la base après le test
            if (!string.IsNullOrEmpty(_dbName))
            {
                using var masterConn = new MySqlConnection("Server=localhost;Port=3306;User=root;Password=!PKdM?MJR9wEfQa;");
                masterConn.Open();
                using var dropCmd = new MySqlCommand($"DROP DATABASE IF EXISTS `{_dbName}`;", masterConn);
                dropCmd.ExecuteNonQuery();
                _dbName = null;
            }

            if (!string.IsNullOrEmpty(_dbAuthName))
            {
                using var masterConn = new MySqlConnection("Server=localhost;Port=3306;User=root;Password=!PKdM?MJR9wEfQa;");
                masterConn.Open();
                using var dropCmd = new MySqlCommand($"DROP DATABASE IF EXISTS `{_dbAuthName}`;", masterConn);
                dropCmd.ExecuteNonQuery();
                _dbAuthName = null;
            }
        }


        public HttpClient CreateAuthenticatedClient(string? userAgent="ArzenalStoreManager",string? token = null)
        {
            var factory = this;
            var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = factory.Server.BaseAddress,
                HandleCookies = true // Laisser HttpClient gérer les cookies retournés par le serveur
            });
            client.DefaultRequestHeaders.Add("X-Forwarded-For", "127.0.0.1");
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);

            // On remplace le handler interne par un delegating handler qui ajoute les cookies

            var loginDto = new LoginRequestDto
            {
                Email = "existing@example.com",
                Password = "hashed-password",
                Fingerprint = "IntegrationTestFingerprint",
                DeviceName = "IntegrationTestDevice"
            };

            var loginResponse = client.PostAsJsonAsync("api/auth/login", loginDto).Result;
            if (!loginResponse.IsSuccessStatusCode)
            {
                var body = loginResponse.Content.ReadAsStringAsync().Result;
                throw new InvalidOperationException($"Login failed with {(int)loginResponse.StatusCode}: {body}");
            }

            // Utilise seulement scheme + host + port pour SetCookies
            var cookieContainer = new CookieContainer();
            var cookies = loginResponse.Headers.GetValues("Set-Cookie");
            foreach (var c in cookies)
            {
                cookieContainer.SetCookies(factory.Server.BaseAddress!, c);
            }

            client.DefaultRequestHeaders.Add("Cookie", string.Join("; ", cookies));
            if (token != null)
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            }

            client.DefaultRequestHeaders.Add("X-Device-Name", "IntegrationTestFingerprint");
            client.DefaultRequestHeaders.Add("Fingerprint", "IntegrationTestFingerprint");

            // Ajoute un header X-App-Id par défaut pour les tests qui appellent les endpoints WPF
            if (!client.DefaultRequestHeaders.Contains("X-App-Id"))
            {
                client.DefaultRequestHeaders.Add("X-App-Id", "integration-test-app");
            }

            return client;
        }


        public HttpClient CreateFakeUserClient(string fingerprint = "Fingerprint")
        {
            // Crée un utilisateur fictif (pas dans la DB)
            var fakeUserId = Guid.NewGuid();
            var fakeEmail = "inexistant@example.com";

            var configuration = Server.Services.GetRequiredService<IConfiguration>();
            // Génère un vrai JWT pour ce faux utilisateur
            var authToken = JwtHelper.GenerateTestJwt(configuration, fakeUserId, fakeEmail);

            // Prépare le refresh token
            var refreshData = new RefreshTokenCookieData
            {
                Token = "fake-refresh-token",
                UserId = fakeUserId
            };

            var cookieHandler = new CookieInjectingHandler(authToken, refreshData, fingerprint)
            {
                InnerHandler = Server.CreateHandler()
            };

            var client = new HttpClient(cookieHandler)
            {
                BaseAddress = Server.BaseAddress ?? new Uri("http://localhost")
            };

            client.DefaultRequestHeaders.Add("X-UserId", fakeUserId.ToString());
            client.DefaultRequestHeaders.Add("X-UserEmail", fakeEmail);

            return client;
        }
    }
}
