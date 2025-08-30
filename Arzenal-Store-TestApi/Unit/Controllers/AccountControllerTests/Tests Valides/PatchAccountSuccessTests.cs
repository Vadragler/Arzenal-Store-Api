using ArzenalStoreApi.Controllers.User;
using ArzenalStoreApi.Services.UserService;
using ArzenalStoreSharedDto.DTOs.AccountDto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace TestArzenalStoreApi.Controllers.AccountControllerTests.Tests_Valides
{
    public class PatchAccountSuccessTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly AccountController _controller;

        public PatchAccountSuccessTests()
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
        public async Task PatchCurrentUser_ReturnsNoContent_WhenPatchSucceeds()
        {
            var dto = new UpdateAccountDto
            {
                Username = "newuser",
                Email = "new@email.com",
                ActualPassword = "oldpass",
                NewPassword = "newpass"
            };

            _userServiceMock.Setup(s => s.PatchAsync("userId123", dto))
                .Returns(Task.CompletedTask);

            var result = await _controller.PatchCurrentUser(dto);

            Assert.IsType<NoContentResult>(result);
        }     
    }
}
