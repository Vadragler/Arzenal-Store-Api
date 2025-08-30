using ArzenalStoreApi.Controllers.Auth;
using ArzenalStoreApi.Data;
using ArzenalStoreApi.Models;
using ArzenalStoreApi.Services.Auth;
using ArzenalStoreApi.Services.Token;
using ArzenalStoreSharedDto.DTOs.AuthDto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TestArzenalStoreApi.Infrastructure;

namespace TestArzenalStoreApi.Integration.Controllers.AuthControllerTests.Tests_Invalides
{
    public class loginAuthErrorTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly AuthController _authController;
        private readonly HttpClient _client;

        public loginAuthErrorTests()
        {
            var factory = new CustomAuthWebApplicationFactory<Program>(); // ou Startup si tu as Startup.cs
            using var scope = factory.Services.CreateScope();
            _client = factory.CreateClient();

            var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

            // Mocks
            _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
            _jwtServiceMock = new Mock<IJwtService>();
            _authServiceMock = new Mock<IAuthService>();
            var refreshTokenServiceMock = new Mock<IRefreshTokenService>();
            var configurationMock = new Mock<IConfiguration>();

            _authController = new AuthController(
                _authServiceMock.Object
                , _refreshTokenServiceMock.Object
                , _jwtServiceMock.Object
            );
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenCredentialsAreInvalid()
        {
            // Arrange
            
            var request = new LoginRequestDto
            {
                Email = "wrong@example.com",
                Password = "badpassword",
                Fingerprint = "some-fingerprint",
                DeviceName = "some-device"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            Assert.Equal("Accès non autorisé", body["error"]);
        }


    }
}
