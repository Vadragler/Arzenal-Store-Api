using Arzenal.Shared.Dtos.DTOs.AuthDto;
using ArzenalApi.Controllers;
using ArzenalApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApi.Controllers.AuthControllerTests.Tests_Valides
{
    public class LoginAuthSuccessTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthController _authController;
        public LoginAuthSuccessTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _authController = new AuthController(_authServiceMock.Object);
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
    }
}
