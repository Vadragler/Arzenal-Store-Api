using ArzenalStoreApi.Controllers.Auth;
using ArzenalStoreApi.Data;
using ArzenalStoreApi.Exceptions;
using ArzenalStoreApi.Services.Auth;
using ArzenalStoreApi.Services.Token;
using ArzenalStoreSharedDto.DTOs.AuthDto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;
using TestArzenalStoreApi.Infrastructure;

namespace TestArzenalStoreApi.Integration.Controllers.AuthControllerTests.Tests_Invalides
{
    public class RegisterAuthErrorTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly AuthController _authController;
        private readonly HttpClient _client;

        public RegisterAuthErrorTests()
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
        public async Task Register_ReturnsBadRequest_WhenTokenIsInvalid()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Username = "newuser",
                Email = "newuser@example.com",
                Password = "StrongPassword123!",
                Token = "invalid-token" // Token invalide
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("Token invalide ou déjà utilisé", body);

        }


        [Fact]
        public async Task Register_ReturnsBadRequest_WhenUsernameIsTaken()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Username = "existinguser",
                Email = "existing@example.com",
                Password = "StrongPassword123!",
                Token = "valid-token"
            };



            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("Email ou nom d'utilisateur déjà utilisé.", body);
        }

    }
}
