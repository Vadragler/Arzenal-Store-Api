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
using TestArzenalStoreApi.Infrastructure;

namespace TestArzenalStoreApi.Controllers.AuthControllerTests.Tests_Valides
{
    public class LoginAuthSuccessTests
    {

        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly AuthController _authController;
        public LoginAuthSuccessTests()
        {
            var factory = new CustomAuthWebApplicationFactory<Program>(); // ou Startup si tu as Startup.cs
            using var scope = factory.Services.CreateScope();

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
        public async Task Login_ReturnsOk_WithToken_WhenCredentialsAreValid()
        {
            // Arrange
           
            var request = new LoginRequestDto { Email = "testuser", Password = "password123" };
            var token = "mocked-jwt-token";
            var refreshToken = "mocked-refresh-token";

            _authServiceMock
                .Setup(service => service.AuthenticateAsync(request.Email, request.Password, It.IsAny<CreateRefreshTokenDto>()))
                .ReturnsAsync((token, refreshToken));

            _authController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _authController.HttpContext.Request.Headers["User-Agent"] = "Test User-Agent";
            _authController.HttpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

            // Act
            var result = await _authController.Login(request);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }
    }
}
