using ArzenalStoreApi.Controllers.User;
using ArzenalStoreApi.Services.UserService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;


namespace TestArzenalStoreApi.Controllers.AccountControllerTests.Tests_Valides
{
    public class DeleteAccountSuccessTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly AccountController _controller;
        public DeleteAccountSuccessTests()
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
        public async Task DeleteCurrentUser_ReturnsNoContent_WhenDeleteSucceeds()
        {
            _userServiceMock.Setup(s => s.DeleteAsync("userId123"))
                .Returns(Task.CompletedTask);

            var result = await _controller.DeleteCurrentUser();

            Assert.IsType<NoContentResult>(result);
        }
    }
}
