using Arzenal.Shared.Dtos.DTOs.RegisterDto;
using Arzenal.Shared.Dtos.DTOs;
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
    public class RegisterAuthSuccessTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthController _authController;

        public RegisterAuthSuccessTests()
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
    }
}
