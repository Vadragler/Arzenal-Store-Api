using Arzenal.Shared.Dtos.DTOs.AppDto;
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
using System.Text.Json;
using System.Threading.Tasks;
using TestApi.Infrastructure;
using Arzenal.Shared.Dtos.DTOs.AccountDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;


namespace TestApi.Controllers.AccountControllerTests.Tests_Valides
{
    public class GetAccountSuccessTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AccountController _controller;
        public GetAccountSuccessTests()
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
            _authServiceMock.Setup(s => s.FindByIdAsync("userId123"))
                .ReturnsAsync(("testuser", "test@email.com"));

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
