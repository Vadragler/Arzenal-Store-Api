using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using ArzenalStoreSharedDto.DTOs.AccountDto;
using ArzenalStoreApi.Controllers.User;
using ArzenalStoreApi.Services.UserService;


namespace TestArzenalStoreApi.Controllers.AccountControllerTests.Tests_Valides
{
    public class GetAccountSuccessTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly AccountController _controller;
        public GetAccountSuccessTests()
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
            var accountDto = new ReadAccountDto 
            { 
                Username = "testuser",
                Email = "test@email.com"
            };
            _userServiceMock.Setup(s => s.FindByIdAsync("userId123"))
                .ReturnsAsync(accountDto);

            var result = await _controller.GetCurrentUser();

            // Fix: Cast the result to OkObjectResult to access its Value property
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = okResult.Value as ReadAccountDto;

            Assert.NotNull(data);
            Assert.Equal("testuser", data.Username);
            Assert.Equal("test@email.com", data.Email);
        }
    }
}
