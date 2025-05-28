using ArzenalApi.Controllers;
using ArzenalApi.Services;
using Arzenal.Shared.Dtos.DTOs.AccountDto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace TestApi.Controllers.AccountControllerTests.Tests_Valides
{
    public class PatchAccountSuccessTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AccountController _controller;

        public PatchAccountSuccessTests()
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
        public async Task PatchCurrentUser_ReturnsOk_WhenPatchSucceeds()
        {
            var dto = new UpdateAccountDto
            {
                Username = "newuser",
                Email = "new@email.com",
                ActualPassword = "oldpass",
                NewPassword = "newpass"
            };

            _authServiceMock.Setup(s => s.PatchAsync("userId123", dto.Username, dto.Email, dto.ActualPassword, dto.NewPassword))
                .ReturnsAsync(true);

            var result = await _controller.PatchCurrentUser(dto);

            Assert.IsType<OkResult>(result);
        }     
    }
}
