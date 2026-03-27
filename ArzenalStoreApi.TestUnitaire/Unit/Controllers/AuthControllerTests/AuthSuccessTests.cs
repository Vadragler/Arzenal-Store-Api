using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Dto.DTOs.AuthDto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ArzenalStoreApi.Controllers.Auth;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Controllers.AuthControllerTests
{
    public class AuthSuccessTests
    {

        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<IRequestInfoProvider> _requestInfoProviderMock;
        private readonly Mock<ICookieService> _cookieServiceMock;
        private readonly AuthController _authController;
        public AuthSuccessTests()
        {
            // Mocks
            _jwtServiceMock = new Mock<IJwtService>();
            _authServiceMock = new Mock<IAuthService>();
            _requestInfoProviderMock = new Mock<IRequestInfoProvider>();

            _authController = new AuthController(
               _authServiceMock.Object,
               _jwtServiceMock.Object,
               _requestInfoProviderMock.Object
            );
        }

        [Fact]
        public async Task Register_ShouldReturn201Created()
        {
            // Arrange
            var dto = new RegisterRequestDto
            {
                Email = "test.email",
                Password = "StrongPassword123!"
            };

            // Act
            var result = await _authController.Register(dto);

            // Assert
            var statusResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status201Created, statusResult.StatusCode);
            _authServiceMock.Verify(s => s.RegisterAsync(dto), Times.Once);
        }

        [Fact]
        public async Task Login_ReturnsOk_WithToken_WhenCredentialsAreValid()
        {
            // Arrange
            var response = new DefaultHttpContext().Response;
            var request = new LoginRequestDto { Email = "testuser", Password = "password123" };
            var token = "mocked-jwt-token";
            var refreshToken = "mocked-refresh-token";

            _authServiceMock
                .Setup(service => service.AuthenticateAsync(response, request.Email, request.Password, It.IsAny<CreateRefreshTokenDto>()))
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
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task Refresh_ShouldReturnOk()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            _authController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            // Act
            var result = await _authController.Refresh();
            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            _authServiceMock.Verify(s => s.RotateRefreshTokenAsync(httpContext), Times.Once);
        }

        [Fact]
        public async Task GenerateRefreshToken_ShouldReturnOk()
        {             
            // Arrange
            var httpContext = new DefaultHttpContext();
            _authController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            var dto = new CreateRefreshTokenDto
            {
                UserId = Guid.NewGuid(),
                DeviceName = "Nom Device",
                Fingerprint = "Fingerprint123"
            };

            // Act
            var result = await _authController.GenerateRefreshToken(dto);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            _authServiceMock.Verify(s => s.GenerateRefreshTokenAsync(httpContext, dto), Times.Once);
        }

        [Fact]
        public async Task Revoke_ShouldReturnOk_WithMessage()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            _authController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _authController.Revoke();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task Logout_ShouldReturnOk_WithMessage()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            _authController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _authController.Logout();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public void Ping_ShouldReturnOk_WithStatusOk()
        {
            // Act
            var result = _authController.Ping();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task CreateWPFToken_ShouldReturnOk_WithToken()
        {
            // Act
            var result = await _authController.Create_App_Login_Token();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

    }
}
