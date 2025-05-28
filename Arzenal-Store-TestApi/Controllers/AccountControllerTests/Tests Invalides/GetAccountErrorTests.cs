using Arzenal.Shared.Dtos.DTOs.AccountDto;
using ArzenalApi.Controllers;
using ArzenalApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace TestApi.Controllers.AccountControllerTests.Tests_Invalides
{
    public class GetAccountErrorTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AccountController _controller;
        public GetAccountErrorTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _controller = new AccountController(_authServiceMock.Object);

            // Simule un utilisateur authentifié
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "userId123")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetCurrentUser_ReturnsOk_WhenUserIsValid()
        {
            // Arrange
            _authServiceMock.Setup(s => s.FindByIdAsync("userId123"))
                .ReturnsAsync(("testuser", "test@email.com"));

            // Act
            var result = await _controller.GetCurrentUser();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<ReadAccountDto>(okResult.Value);
            Assert.Equal("testuser", data.Username);
            Assert.Equal("test@email.com", data.Email);
        }

    }
}
