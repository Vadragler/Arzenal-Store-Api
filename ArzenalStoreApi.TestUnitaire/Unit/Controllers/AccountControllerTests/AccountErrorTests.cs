using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Dto.DTOs.AccountDto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using ArzenalStoreApi.Controllers.User;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Controllers.AccountControllerTests
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
        public async Task GetCurrentUser_Throws_WhenUserNotFound()
        {
            // Arrange
            _userServiceMock.Setup(s => s.FindByIdAsync("userId123"))
                .ThrowsAsync(new NotFoundException("Utilisateur introuvable"));

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _controller.GetCurrentUser());
        }

        [Fact]
        public async Task PatchCurrentUser_Throws_WhenServiceFails()
        {
            // Arrange
            var dto = new UpdateAccountDto
            {
                Username = "invalid",
                Email = "invalidemail",
                ActualPassword = "wrongpass",
                NewPassword = "newpass"
            };

            _userServiceMock.Setup(s => s.PatchAsync("userId123", dto))
                .ThrowsAsync(new ValidationException("Données invalides"));

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _controller.PatchCurrentUser(dto));
        }

        [Fact]
        public async Task DeleteCurrentUser_Throws_WhenServiceFails()
        {
            // Arrange
            _userServiceMock.Setup(s => s.DeleteAsync("userId123"))
                .ThrowsAsync(new UnauthorizedAccessException("Utilisateur introuvable"));

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.DeleteCurrentUser());
        }

    }
}
