using Arzenal.Store.Api.Service.Interfaces;
using ArzenalStoreApi.Controllers.Auth;
using Moq;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Controllers.InviteControllerTests
{
    public class InviteErrorTests
    {
        private readonly Mock<IInviteService> _inviteServiceMock;
        private readonly InviteController _controller;

        public InviteErrorTests()
        {
            _inviteServiceMock = new Mock<IInviteService>();
            _controller = new InviteController(_inviteServiceMock.Object);
        }

        [Fact]
        public async Task ValidateToken_ThrowsException_WhenTokenInvalid()
        {
            // Arrange
            string token = "invalid-token";
            _inviteServiceMock.Setup(s => s.ValidateInviteAsync(token))
                .ThrowsAsync(new UnauthorizedAccessException("Token invalide ou expiré."));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.ValidateToken(token));
            Assert.Equal("Token invalide ou expiré.", ex.Message);
        }

        [Fact]
        public async Task GenerateToken_ThrowsException_WhenServiceFails()
        {
            // Arrange
            string email = "fail@example.com";
            _inviteServiceMock.Setup(s => s.CreateInviteAsync(email))
                .ThrowsAsync(new Exception("Erreur création invite"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.GenerateToken(email));
            Assert.Equal("Erreur création invite", ex.Message);
        }

        [Fact]
        public async Task UseToken_ThrowsException_WhenTokenInvalid()
        {
            // Arrange
            Guid Gtoken = Guid.NewGuid();
            string token = Gtoken.ToString();
            _inviteServiceMock.Setup(s => s.UseInviteAsync(token))
                .ThrowsAsync(new UnauthorizedAccessException("Token invalide ou déjà utilisé."));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.UseToken(token));
            Assert.Equal("Token invalide ou déjà utilisé.", ex.Message);
        }
    }
}
