using ArzenalApi.Controllers;
using ArzenalApi.DTOs;
using ArzenalApi.DTOs.AuthDto;
using ArzenalApi.DTOs.RegisterDto;
using ArzenalApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Moq;
using Xunit;

namespace TestApi.Controllers.AuthControllerTests
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthController _authController;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _authController = new AuthController(_authServiceMock.Object);
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
                Token = ""
            };

            _authServiceMock
                .Setup(service => service.RegisterAsync(request))
                .ReturnsAsync((true, "Inscription réussie"));

            // Act
            var result = await _authController.Register(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<RegisterResponseDto>(okResult.Value);
            Assert.Equal("Inscription réussie", response.Message);// Fix: Access the Value property of OkObjectResult
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
                Token = ""
            };

            _authServiceMock
                .Setup(service => service.RegisterAsync(It.IsAny<RegisterRequestDto>()))
                .ReturnsAsync((false, "Username is already taken")); // Fix: Adjusted to match the expected return type of RegisterAsync

            // Act
            var result = await _authController.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Username is already taken", badRequestResult.Value);
        }


        [Fact]
        public async Task Login_ReturnsOk_WithToken_WhenCredentialsAreValid()
        {
            // Arrange
            var request = new LoginRequestDto { Email = "testuser", Password = "password123" };
            var token = "mocked-jwt-token";
            _authServiceMock
                .Setup(service => service.AuthenticateAsync(request.Email, request.Password))
                .ReturnsAsync(token);

            // Act
            var result = await _authController.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<LoginResponseDto>(okResult.Value);
            Assert.Equal(token, response.Token);


        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenCredentialsAreInvalid()
        {
            // Arrange
            var request = new LoginRequestDto { Email = "testuser", Password = "wrongpassword" };
            _authServiceMock
                .Setup(service => service.AuthenticateAsync(request.Email, request.Password))
                .ReturnsAsync((string)null);

            // Act
            var result = await _authController.Login(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid email or password", unauthorizedResult.Value);
        }
    }
}
