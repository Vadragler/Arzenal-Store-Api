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

namespace TestApi.Controllers.AccountControllerTests.Tests_Valides
{
    public class DeleteAccountSuccessTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AccountController _controller;
        public DeleteAccountSuccessTests()
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
        public async Task DeleteCurrentUser_ReturnsOk_WhenDeleteSucceeds()
        {
            _authServiceMock.Setup(s => s.DeleteAsync("userId123"))
                .ReturnsAsync(true);

            var result = await _controller.DeleteCurrentUser();

            Assert.IsType<OkResult>(result);
        }
    }
}
