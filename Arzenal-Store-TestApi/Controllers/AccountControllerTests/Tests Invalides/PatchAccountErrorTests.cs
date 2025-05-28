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
    public class PatchAccountErrorTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AccountController _controller;

        public PatchAccountErrorTests()
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
        public async Task PatchCurrentUser_ReturnsBadRequest_WhenPatchFails()
        {
            var dto = new UpdateAccountDto
            {
                Username = "newuser",
                Email = "new@email.com",
                ActualPassword = "oldpass",
                NewPassword = "newpass"
            };

            _authServiceMock.Setup(s => s.PatchAsync("userId123", dto.Username, dto.Email, dto.ActualPassword, dto.NewPassword))
                .ReturnsAsync(false);

            var result = await _controller.PatchCurrentUser(dto);

            Assert.IsType<BadRequestResult>(result);
        }
    }
}
