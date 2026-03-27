using Arzenal.Dto.DTOs.AuthDto;
using System.Net;
using System.Net.Http.Json;
using Arzenal.Store.Api.TestIntegration.Infrastructure;

namespace Arzenal.Store.Api.TestIntegration.Integration.Controllers.AuthControllerTests
{
    public class AuthErrorTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly HttpClient _clientUnauthorized;

        public AuthErrorTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateAuthenticatedClient();
            _clientUnauthorized = _factory.CreateClient();
        }

        // ------------------- REGISTER -------------------
        [Fact]
        public async Task Register_ReturnsUnauthorized_WhenTokenIsInvalid()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Email = "fail@test.com",
                Username = "failUser",
                Password = "Password123!",
                Token = "invalid-token"
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/auth/register", request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ------------------- LOGIN -------------------
        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenCredentialsAreInvalid()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "integration@test.com",
                Password = "WrongPassword",
                DeviceName = "IntegrationTestDevice",
                Fingerprint = "IntegrationTestFingerprint"
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/auth/login", request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ------------------- REFRESH -------------------
        [Fact]
        public async Task Refresh_ReturnsUnauthorized_WhenRefreshTokenMissing()
        {
            var clientWithoutCookie = _factory.CreateClient();

            // Act
            var response = await clientWithoutCookie.PostAsync("api/auth/refresh", null);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ------------------- REVOKE -------------------
        [Fact]
        public async Task Revoke_ReturnsUnauthorized_WhenNotAuthorized()
        {

            // Act
            var response = await _clientUnauthorized.PostAsync("api/auth/revoke", null);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ------------------- LOGOUT -------------------
        [Fact]
        public async Task Logout_ReturnsUnauthorized_WhenNotAuthorized()
        {
            // Act
            var response = await _clientUnauthorized.PostAsync("api/auth/logout", null);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ------------------- PING -------------------
        [Fact]
        public async Task Ping_ReturnsUnauthorized_WhenNotAuthorized()
        {
            // Act
            var response = await _clientUnauthorized.GetAsync("api/auth/ping");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ------------------- CREATE WPF TOKEN -------------------
        [Fact]
        public async Task CreateWPFToken_ReturnsUnauthorized_WhenNotAuthorized()
        {
            // Act
            var response = await _clientUnauthorized.GetAsync("api/auth/create-wpf-token");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
