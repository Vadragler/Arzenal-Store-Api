using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Dto.DTOs.AccountDto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using ArzenalStoreApi.Controllers.User;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Controllers.AccountControllerTests
{
    public class AccountSuccessTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly AccountController _controller;
        public AccountSuccessTests()
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
            };
            _userServiceMock.Setup(s => s.FindByIdAsync("userId123"))
                .ReturnsAsync(accountDto);

            // Act
            var result = await _controller.GetCurrentUser();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = okResult.Value as ReadAccountDto;

            Assert.NotNull(data);
            Assert.Equal("testuser", data.Username);
            Assert.Equal("test@email.com", data.Email);
        }

        [Fact]
        public async Task PatchCurrentUser_ReturnsNoContent_WhenPatchSucceeds()
        {
            // Arrange
            var dto = new UpdateAccountDto
            {
                Username = "newuser",
                Email = "new@email.com",
                ActualPassword = "oldpass",
                NewPassword = "newpass"
            };

            _userServiceMock.Setup(s => s.PatchAsync("userId123", dto))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.PatchCurrentUser(dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteCurrentUser_ReturnsNoContent_WhenDeleteSucceeds()
        {
            // Arrange
            _userServiceMock.Setup(s => s.DeleteAsync("userId123"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteCurrentUser();

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
