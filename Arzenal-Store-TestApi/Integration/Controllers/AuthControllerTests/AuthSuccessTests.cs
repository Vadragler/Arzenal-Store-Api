using Arzenal.Dto.DTOs.AuthDto;
using System.Net;
using System.Net.Http.Json;
using Arzenal.Store.Api.TestIntegration.Infrastructure;

namespace Arzenal.Store.Api.TestIntegration.Integration.Controllers.AuthControllerTests
{
    public class AuthSuccessTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public AuthSuccessTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateAuthenticatedClient();
            
        }

        // ------------------- REGISTER -------------------
        [Fact]
        public async Task Register_ReturnsCreated_WhenDataIsValid()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Email = "integration@test.com",
                Username = "integrationUser",
                Password = "Password123!",
                Token = "valid-token"
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/auth/register", request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        // ------------------- LOGIN -------------------
        [Fact]
        public async Task Login_ReturnsOk_WhenCredentialsAreValid()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "existing@example.com",
                Password = "hashed-password",
                DeviceName = "IntegrationTestDevice",
                Fingerprint = "IntegrationTestFingerprint"
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/auth/login", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Connexion réussie", content);
        }

        [Fact]
        public async Task GenerateRefreshToken_ReturnsOK_WhenCredentialAreValid()
        {
            // Arrange
            await using var factory = new CustomWebApplicationFactory<Program>();
            var initialResponse = await _client.GetAsync("api/auth/create-app-login-token");
            initialResponse.EnsureSuccessStatusCode();
            var json = await initialResponse.Content.ReadAsStringAsync();

            var token = json.Split('"')[3];

            // Utiliser le paramètre nommé pour passer le token afin d'éviter qu'il soit interprété comme userAgent
            var client = factory.CreateAuthenticatedClient(token: token);

            CreateRefreshTokenDto requet = new CreateRefreshTokenDto
            {
                UserId = Guid.Parse("d3f1a9e2-5b6c-4f7d-9a12-8c3e4b5f6789"),
                DeviceName = "IntegrationTestDevice",
                Fingerprint = "IntegrationTestFingerprint",
                CreatedByIp = "127.0.0.1"
            };

            // Act
            var refreshResponse = await client.PostAsJsonAsync("api/auth/GenerateRefreshToken", requet);

            // Assert
            Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        }

        // ------------------- REFRESH -------------------
        [Fact]
        public async Task Refresh_ReturnsOk_WhenRefreshTokenExists()
        {
            await using var factory = new CustomWebApplicationFactory<Program>();
            var client = factory.CreateAuthenticatedClient();
            // Act
            var response = await client.PostAsync("api/auth/refresh", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
       
        // ------------------- REVOKE -------------------
        [Fact]
        public async Task Revoke_ReturnsOk_WhenAuthorized()
        {
            // Arrange
            using var factory = new CustomWebApplicationFactory<Program>();
            var client = factory.CreateAuthenticatedClient();
            // Act
            var response = await client.PostAsync("api/auth/revoke", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        } 

        // ------------------- LOGOUT -------------------
        [Fact]
        public async Task Logout_ReturnsOk_WhenAuthorized()
        {
            // Arrange
            using var factory = new CustomWebApplicationFactory<Program>();
            var client = factory.CreateAuthenticatedClient("Chrome");

            // Act
            var response = await client.PostAsync("api/auth/logout", null);


            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ------------------- PING -------------------
        [Fact]
        public async Task Ping_ReturnsOk_WhenAuthorized()
        {
            // Act
            var response = await _client.GetAsync("api/auth/ping");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ------------------- CREATE WPF TOKEN -------------------
        [Fact]
        public async Task CreateWPFToken_ReturnsOk_WhenAuthorized()
        {
            // Act & Assert
            var response = await _client.GetAsync("api/auth/create-app-login-token");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("token", json);
        }
    }
}
