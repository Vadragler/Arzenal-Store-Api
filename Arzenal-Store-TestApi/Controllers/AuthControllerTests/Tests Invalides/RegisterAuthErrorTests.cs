using Arzenal.Shared.Dtos.DTOs.RegisterDto;
using ArzenalApi.Controllers;
using ArzenalApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApi.Controllers.AuthControllerTests.Tests_Invalides
{
    public class RegisterAuthErrorTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthController _authController;

        public RegisterAuthErrorTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _authController = new AuthController(_authServiceMock.Object);
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

            _authServiceMock
                .Setup(service => service.RegisterAsync(It.IsAny<RegisterRequestDto>()))
                .ReturnsAsync((false, "Invalid token")); // Simule une erreur liée au token

            // Act
            var result = await _authController.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid token", badRequestResult.Value);
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

    }
}
