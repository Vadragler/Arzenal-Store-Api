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
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Arzenal.Store.Api.TestIntegration.Infrastructure
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private const string JwtSecret = "integration-jwt-secret-must-be-at-least-thirty-two-characters";
        private const string JwtIssuer = "arzenal-integration";
        private const string AppLoginJwtSecret = "integration-app-jwt-secret-must-be-at-least-thirty-two-characters";
        private const string AppLoginJwtIssuer = "arzenal-integration-app";
        private static readonly IReadOnlyDictionary<string, string?> TestConfiguration =
            new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = JwtSecret,
                ["Jwt:Issuer"] = JwtIssuer,
                ["Jwt-App-Login:SecretKey"] = AppLoginJwtSecret,
                ["Jwt-App-Login:Issuer"] = AppLoginJwtIssuer,
                ["RefreshToken:HashKey"] = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
                ["ConnectionStrings:StoreConnection"] = "Server=localhost;Database=placeholder;User Id=placeholder;Password=placeholder;",
                ["ConnectionStrings:AuthConnection"] = "Server=localhost;Database=placeholder;User Id=placeholder;Password=placeholder;",
                ["AppLogin:AllowedAppIds:0"] = "ArzenalStoreManager",
                ["AppLogin:AllowedAppIds:1"] = "integration-test-app",
                ["Database:ApplyMigrationsOnStartup"] = "false"
            };

        private MySqlTestDatabase? _databases;
        private string? _connectionString;
        private string? _connectionAuthString;
        public string StoragePath { get; private set; } = Path.Combine(Path.GetTempPath(), "ArzenalTest", Guid.NewGuid().ToString());

        public new IServiceProvider Services => Server.Services;

        public CustomWebApplicationFactory()
        {
            ConfigureTestProcessEnvironment();
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            var host = base.CreateHost(builder);
            try
            {
                using var scope = host.Services.CreateScope();
                var appDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
                appDb.Database.Migrate();
                authDb.Database.Migrate();
                ApplicationDbSeeder.Seed(appDb);
                AuthDbSeeder.Seed(authDb);
                return host;
            }
            catch
            {
                host.Dispose();
                throw;
            }
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureLogging(logging => logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning));
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(TestConfiguration);
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

                CreateIsolatedDatabases();

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseMySql(_connectionString, new MySqlServerVersion(new Version(8, 0, 33))));
                services.AddDbContext<AuthDbContext>(options =>
                    options.UseMySql(_connectionAuthString, new MySqlServerVersion(new Version(8, 0, 33))));

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

        private static void ConfigureTestProcessEnvironment()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            foreach (var (key, value) in TestConfiguration)
            {
                Environment.SetEnvironmentVariable(key.Replace(":", "__", StringComparison.Ordinal), value);
            }
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
                    CleanupOwnedResources();
                }
            }
        }

        private void CreateIsolatedDatabases()
        {
            if (_databases is not null) return;
            _databases = MySqlTestDatabase.CreateAsync(includeAuth: true).GetAwaiter().GetResult();
            _connectionString = _databases.StoreConnectionString;
            _connectionAuthString = _databases.AuthConnectionString;
        }

        public override async ValueTask DisposeAsync()
        {
            try { await base.DisposeAsync(); }
            finally { CleanupOwnedResources(); }
        }

        private void CleanupOwnedResources()
        {
            try { Interlocked.Exchange(ref _databases, null)?.Dispose(); }
            finally
            {
                if (Directory.Exists(StoragePath))
                {
                    var path = Path.GetFullPath(StoragePath);
                    var prefix = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "ArzenalTest")) + Path.DirectorySeparatorChar;
                    var comparison = System.OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                    if (!path.StartsWith(prefix, comparison) || !Guid.TryParse(Path.GetFileName(path), out _))
                        throw new InvalidOperationException("Répertoire de test non autorisé pour le nettoyage.");
                    Directory.Delete(path, recursive: true);
                }
            }
        }


        public HttpClient CreateAuthenticatedClient(string? userAgent = "ArzenalStoreManager", string? token = null)
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
