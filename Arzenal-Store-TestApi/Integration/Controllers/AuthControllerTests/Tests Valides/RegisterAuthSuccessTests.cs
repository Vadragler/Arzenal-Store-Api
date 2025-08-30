using Microsoft.AspNetCore.Mvc;
using Moq;
using ArzenalStoreApi.Data;
using Microsoft.Extensions.Configuration;
using TestArzenalStoreApi.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using ArzenalStoreSharedDto.DTOs.AuthDto;
using ArzenalStoreApi.Services.Token;
using ArzenalStoreApi.Services.Auth;
using ArzenalStoreApi.Controllers.Auth;
using System.Net.Http.Json;
using System.Net;

namespace TestArzenalStoreApi.Integration.Controllers.AuthControllerTests.Tests_Valides
{
    public class RegisterAuthSuccessTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly AuthController _authController;
        private readonly HttpClient _client;
        public RegisterAuthSuccessTests()
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
              _authServiceMock.Object,
                _refreshTokenServiceMock.Object,
                 _jwtServiceMock.Object
            );
        }

        [Fact]
        public async Task Register_ReturnsOk_WhenRegistrationIsSuccessful()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "StrongPassword123!",
                Token = "valid-token"
            };

            // Act
            var httpResponse = await _client.PostAsJsonAsync("/api/auth/register", request);
            
            Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
        }
    }
}
