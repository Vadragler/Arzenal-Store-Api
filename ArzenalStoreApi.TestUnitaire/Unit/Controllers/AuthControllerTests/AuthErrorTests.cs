using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Dto.DTOs.AuthDto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ArzenalStoreApi.Controllers.Auth;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Controllers.AuthControllerTests
{
    public class AuthErrorTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<IRequestInfoProvider> _requestInfoProviderMock;
        private readonly Mock<ICookieService> _cookieServiceMock;
        private readonly AuthController _authController;

        public AuthErrorTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _jwtServiceMock = new Mock<IJwtService>();
            _requestInfoProviderMock = new Mock<IRequestInfoProvider>();

            _authController = new AuthController(
                _authServiceMock.Object,
                _jwtServiceMock.Object,
                _requestInfoProviderMock.Object
            );
        }

        [Fact]
        public async Task Register_ShouldReturn500_WhenServiceThrows()
        {
            // Arrange
            var dto = new RegisterRequestDto { Email = "fail@test", Password = "1234" };
            _authServiceMock.Setup(s => s.RegisterAsync(dto)).ThrowsAsync(new Exception("Erreur d'inscription"));

            // Act & Assert
            var result = await Assert.ThrowsAsync<Exception>(() => _authController.Register(dto));
            Assert.Equal("Erreur d'inscription", result.Message);
        }

        [Fact]
        public async Task Refresh_ShouldThrow_WhenServiceFails()
        {
            // Arrange
            var context = new DefaultHttpContext();
            _authController.ControllerContext.HttpContext = context;
            _authServiceMock
                .Setup(s => s.RotateRefreshTokenAsync(context))
                .ThrowsAsync(new InvalidOperationException("Token invalide"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _authController.Refresh());
            Assert.Equal("Token invalide", ex.Message);
        }

        [Fact]
        public async Task GenerateRefreshToken_ShouldThrow_WhenDtoInvalid()
        {
            // Arrange
            var context = new DefaultHttpContext();
            _authController.ControllerContext.HttpContext = context;
            var dto = new CreateRefreshTokenDto(); // vide

            _authServiceMock
                .Setup(s => s.GenerateRefreshTokenAsync(context, dto))
                .ThrowsAsync(new ArgumentException("Données invalides"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _authController.GenerateRefreshToken(dto));
            Assert.Equal("Données invalides", ex.Message);
        }

        [Fact]
        public async Task Revoke_ShouldThrow_WhenServiceFails()
        {
            // Arrange
            var context = new DefaultHttpContext();
            _authController.ControllerContext.HttpContext = context;

            _authServiceMock
                .Setup(s => s.RevokeRefreshTokenAsync(context))
                .ThrowsAsync(new Exception("Erreur de révocation"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _authController.Revoke());
            Assert.Equal("Erreur de révocation", ex.Message);
        }

        [Fact]
        public async Task Logout_ShouldThrow_WhenServiceFails()
        {
            // Arrange
            var context = new DefaultHttpContext();
            _authController.ControllerContext.HttpContext = context;

            _authServiceMock
                .Setup(s => s.LogoutAsync(context))
                .ThrowsAsync(new Exception("Erreur de déconnexion"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _authController.Logout());
            Assert.Equal("Erreur de déconnexion", ex.Message);
        }

        [Fact]
        public async Task CreateWPFToken_ShouldThrow_WhenServiceFails()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            _authController.ControllerContext.HttpContext = httpContext;

            _jwtServiceMock
                .Setup(s => s.GenerateJwtTokenForAppAsync(httpContext.Request))
                .Throws(new InvalidOperationException("Erreur de génération du token"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _authController.Create_App_Login_Token());
            Assert.Equal("Erreur de génération du token", ex.Message);
        }

        [Fact]
        public void Ping_ShouldNeverThrow()
        {
            // Act
            var result = _authController.Ping();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
