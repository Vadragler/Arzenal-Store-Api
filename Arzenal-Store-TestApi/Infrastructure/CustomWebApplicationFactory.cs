using Arzenal.Dto.DTOs.AuthDto;
using Arzenal.Store.Api.Domain.Models.Requests;
using Arzenal.Store.Api.Infrastructure.Data;
using Arzenal.Store.Api.TestIntegration.Infrastructure.DbSeeder;
using ArzenalStoreInfrastructure.Configurations;
using ArzenalStoreInfrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Testcontainers.MySql;

namespace Arzenal.Store.Api.TestIntegration.Infrastructure
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private const string MySqlImage = "mysql:8.0.33";
        private const string JwtSecret = "integration-jwt-secret-must-be-at-least-thirty-two-characters";
        private const string JwtIssuer = "arzenal-integration";
        private const string AppLoginJwtSecret = "integration-app-jwt-secret-must-be-at-least-thirty-two-characters";
        private const string AppLoginJwtIssuer = "arzenal-integration-app";

        private readonly MySqlContainer _databaseContainer;
        private readonly string _dbName;
        private readonly string _dbAuthName;
        private string? _connectionString;
        private string? _connectionAuthString;
        private bool _containerStarted;
        public string StoragePath { get; private set; } = Path.Combine(Path.GetTempPath(), "ArzenalTest", Guid.NewGuid().ToString());

        public new IServiceProvider Services => Server.Services;

        public CustomWebApplicationFactory()
        {
            // MySQL limite les noms de verrous à 64 caractères. EF Core dérive
            // le nom du verrou à partir du nom de la base, donc ce suffixe doit rester court.
            var suffix = Guid.NewGuid().ToString("N")[..16];
            _dbName = $"arzenal_test_{suffix}";
            _dbAuthName = $"arzenal_auth_test_{suffix}";
            _databaseContainer = new MySqlBuilder(MySqlImage)
                .WithDatabase(_dbName)
                .WithUsername("root")
                .WithPassword($"Test-{Guid.NewGuid():N}")
                .Build();
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            return base.CreateHost(builder);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:SecretKey"] = JwtSecret,
                    ["Jwt:Issuer"] = JwtIssuer,
                    ["Jwt-App-Login:SecretKey"] = AppLoginJwtSecret,
                    ["Jwt-App-Login:Issuer"] = AppLoginJwtIssuer,
                    ["RefreshToken:HashKey"] = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
                    ["ConnectionStrings:StoreConnection"] = "Server=localhost;Database=placeholder;User Id=placeholder;Password=placeholder;",
                    ["ConnectionStrings:AuthConnection"] = "Server=localhost;Database=placeholder;User Id=placeholder;Password=placeholder;",
                    ["AppLogin:AllowedAppIds:0"] = "ArzenalStoreManager",
                    ["AppLogin:AllowedAppIds:1"] = "integration-test-app"
                });
            });

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
                ConfigureJwtForTests(services);

                StartDatabaseContainer();

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseMySql(_connectionString, new MySqlServerVersion(new Version(8, 0, 33))));
                services.AddDbContext<AuthDbContext>(options =>
                    options.UseMySql(_connectionAuthString, new MySqlServerVersion(new Version(8, 0, 33))));

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

        private static void ConfigureJwtForTests(IServiceCollection services)
        {
            services.PostConfigure<JwtBearerOptions>("CookieJwt", options =>
            {
                options.TokenValidationParameters.IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
                options.TokenValidationParameters.ValidIssuer = JwtIssuer;
                options.TokenValidationParameters.ValidAudiences = new[] { "web", "ArzenalStoreManager" };
            });

            services.PostConfigure<JwtBearerOptions>("HeaderJwt", options =>
            {
                options.TokenValidationParameters.IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppLoginJwtSecret));
                options.TokenValidationParameters.ValidIssuer = AppLoginJwtIssuer;
                options.TokenValidationParameters.ValidAudience = "app-login";
            });
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                base.Dispose(disposing);
            }
            finally
            {
                if (disposing)
                {
                    DisposeContainer(_databaseContainer);
                }
            }
        }

        private void StartDatabaseContainer()
        {
            if (_containerStarted)
            {
                return;
            }

            try
            {
                _databaseContainer.StartAsync().GetAwaiter().GetResult();
                _connectionString = _databaseContainer.GetConnectionString();

                var serverConnectionStringBuilder = new MySqlConnectionStringBuilder(_connectionString)
                {
                    Database = "mysql"
                };

                using var serverConnection = new MySqlConnection(serverConnectionStringBuilder.ConnectionString);
                serverConnection.Open();

                using var createAuthDatabaseCommand = new MySqlCommand($"CREATE DATABASE `{_dbAuthName}`;", serverConnection);
                createAuthDatabaseCommand.ExecuteNonQuery();

                _connectionAuthString = new MySqlConnectionStringBuilder(_connectionString)
                {
                    Database = _dbAuthName
                }.ConnectionString;
                _containerStarted = true;
            }
            catch
            {
                DisposeContainer(_databaseContainer);
                throw;
            }
        }

        private static void DisposeContainer(MySqlContainer container)
        {
            container.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }


        public HttpClient CreateAuthenticatedClient(string? userAgent="ArzenalStoreManager",string? token = null)
        {
            var factory = this;
            var loginClient = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = factory.Server.BaseAddress,
                HandleCookies = false
            });
            loginClient.DefaultRequestHeaders.Add("X-Forwarded-For", $"10.0.{Random.Shared.Next(1, 255)}.{Random.Shared.Next(1, 255)}");
            loginClient.DefaultRequestHeaders.Add("User-Agent", userAgent);

            // On remplace le handler interne par un delegating handler qui ajoute les cookies

            var loginDto = new LoginRequestDto
            {
                Email = "existing@example.com",
                Password = "hashed-password",
                Fingerprint = "IntegrationTestFingerprint",
                DeviceName = "IntegrationTestDevice"
            };

            var loginResponse = loginClient.PostAsJsonAsync("api/auth/login", loginDto).Result;
            if (!loginResponse.IsSuccessStatusCode)
            {
                var body = loginResponse.Content.ReadAsStringAsync().Result;
                throw new InvalidOperationException($"Login failed with {(int)loginResponse.StatusCode}: {body}");
            }

            var accessTokenCookie = loginResponse.Headers.GetValues("Set-Cookie")
                .Select(setCookie => setCookie.Split(';', 2)[0])
                .SingleOrDefault(cookie => cookie.StartsWith("authToken=", StringComparison.Ordinal));
            if (accessTokenCookie is null)
                throw new InvalidOperationException("Le login de test n'a pas retourné le cookie authToken.");

            var accessToken = accessTokenCookie["authToken=".Length..];
            ValidateAccessToken(accessToken);

            var refreshTokenCookie = loginResponse.Headers.GetValues("Set-Cookie")
                .Select(setCookie => setCookie.Split(';', 2)[0])
                .SingleOrDefault(cookie => cookie.StartsWith("refreshToken=", StringComparison.Ordinal));
            if (refreshTokenCookie is null)
            {
                throw new InvalidOperationException("Le login de test n'a pas retourné le cookie refreshToken.");
            }

            var client = new HttpClient(new CookieInjectingHandler(
                accessToken,
                refreshTokenCookie["refreshToken=".Length..],
                token ?? accessToken,
                "IntegrationTestFingerprint")
            {
                InnerHandler = factory.Server.CreateHandler()
            })
            {
                BaseAddress = factory.Server.BaseAddress
            };

            client.DefaultRequestHeaders.Add("X-Forwarded-For", $"10.0.{Random.Shared.Next(1, 255)}.{Random.Shared.Next(1, 255)}");
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            client.DefaultRequestHeaders.Add("X-Device-Name", "IntegrationTestDevice");
            client.DefaultRequestHeaders.Add("Fingerprint", "IntegrationTestFingerprint");

            // Ajoute un header X-App-Id par défaut pour les tests qui appellent les endpoints WPF
            if (!client.DefaultRequestHeaders.Contains("X-App-Id"))
            {
                client.DefaultRequestHeaders.Add("X-App-Id", "integration-test-app");
            }

            return client;
        }

        private void ValidateAccessToken(string accessToken)
        {
            var configuration = Services.GetRequiredService<IConfiguration>();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!)),
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudiences = new[] { "web", "ArzenalStoreManager" },
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                new JwtSecurityTokenHandler().ValidateToken(accessToken, validationParameters, out _);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Le JWT émis par le login de test ne passe pas sa propre validation.", exception);
            }
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
