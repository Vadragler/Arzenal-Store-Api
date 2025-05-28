using ArzenalApi.Controllers;
using Arzenal.Shared.Dtos.DTOs;
using Arzenal.Shared.Dtos.DTOs.AuthDto;
using Arzenal.Shared.Dtos.DTOs.RegisterDto;
using ArzenalApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Moq;
using Xunit;

namespace TestApi.Controllers.AuthControllerTests.Tests_Invalides
{
    public class loginAuthErrorTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthController _authController;

        public loginAuthErrorTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _authController = new AuthController(_authServiceMock.Object);
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
