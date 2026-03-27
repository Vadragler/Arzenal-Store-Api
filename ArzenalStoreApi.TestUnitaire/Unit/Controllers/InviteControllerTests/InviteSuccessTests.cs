using Arzenal.Store.Api.Service.Interfaces;
using ArzenalStoreApi.Controllers.Auth;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Controllers.InviteControllerTests
{
    public class InviteSuccessTests
    {
        private readonly Mock<IInviteService> _inviteServiceMock;
        private readonly InviteController _controller;

        public InviteSuccessTests()
        {
            _inviteServiceMock = new Mock<IInviteService>();
            _controller = new InviteController(_inviteServiceMock.Object);
        }

        [Fact]
        public async Task ValidateToken_ReturnsOk_WhenTokenIsValid()
        {
            // Arrange
            string token = "valid-token";
            _inviteServiceMock.Setup(s => s.ValidateInviteAsync(token)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ValidateToken(token);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GenerateToken_ReturnsOk_WithInviteLink()
        {
            // Arrange
            string email = "test@example.com";
            string inviteLink = "http://arzenaldev.duckdns.org/signup?token=12345";
            _inviteServiceMock.Setup(s => s.CreateInviteAsync(email)).ReturnsAsync(inviteLink);

            // Act & Assert
            var result = await _controller.GenerateToken(email);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            Assert.Equal(data["Link"], inviteLink);
        }

        [Fact]
        public async Task UseToken_ReturnsOk_WhenTokenIsValid()
        {
            // Arrange
            Guid Gtoken = Guid.NewGuid();
            string token = Gtoken.ToString();
            _inviteServiceMock.Setup(s => s.UseInviteAsync(token)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UseToken(token);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
