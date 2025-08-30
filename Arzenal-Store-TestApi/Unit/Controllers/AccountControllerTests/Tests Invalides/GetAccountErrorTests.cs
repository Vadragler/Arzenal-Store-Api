using ArzenalStoreApi.Controllers.User;
using ArzenalStoreApi.Services.UserService;
using ArzenalStoreSharedDto.DTOs.AccountDto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace TestArzenalStoreApi.Integration.Controllers.AccountControllerTests.Tests_Invalides
{
    public class GetAccountErrorTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly AccountController _controller;
        public GetAccountErrorTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _controller = new AccountController(_userServiceMock.Object);

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
            var accountDto = new ReadAccountDto
            {
                Username = "testuser",
                Email = "test@email.com"
                // Ajoutez d'autres propriétés si nécessaire
            };
            _userServiceMock.Setup(s => s.FindByIdAsync("userId123"))
                .ReturnsAsync(accountDto);

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
