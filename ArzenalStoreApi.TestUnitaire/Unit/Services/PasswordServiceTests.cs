using Arzenal.Store.Api.Service.Services.Auth.Passwords;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Services
{
    public class PasswordServiceTests
    {
        [Fact]
        public void Hash_ShouldReturnNonEmptyHash()
        {
            // Arrange
            var service = new PasswordService();
            var password = "MySecret123";

            // Act
            var hash = service.Hash(password);

            // Assert
            Assert.False(string.IsNullOrEmpty(hash));
        }

        [Fact]
        public void Verify_ShouldReturnTrueForCorrectPassword_AndFalseForIncorrect()
        {
            // Arrange
            var service = new PasswordService();
            var password = "MySecret123";

            // Act
            var hash = service.Hash(password);

            // Assert
            Assert.True(service.Verify(password, hash));
            Assert.False(service.Verify("WrongPassword", hash));
        }
    }
}
