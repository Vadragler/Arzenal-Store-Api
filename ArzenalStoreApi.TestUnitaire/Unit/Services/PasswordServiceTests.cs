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
            Assert.StartsWith("$argon2id$v=19$m=65536,t=3,p=1$", hash);
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

        [Fact]
        public void Verify_ShouldReturnFalseForMalformedOrLegacyBcryptHash()
        {
            var service = new PasswordService();

            Assert.False(service.Verify("MySecret123", "$2b$12$invalidbcrypt"));
            Assert.False(service.Verify("MySecret123", "$argon2id$v=19$m=1,t=1,p=1$invalid$invalid"));
        }
    }
}
