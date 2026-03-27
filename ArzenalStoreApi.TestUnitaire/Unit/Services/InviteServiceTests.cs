using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.Service.Services.Auth.Invites;
using Microsoft.EntityFrameworkCore;
using Arzenal.Store.Api.Infrastructure.Data;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Services
{
    public class InviteServiceTests
    {
        private AuthDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new AuthDbContext(options);
        }

        [Fact]
        public async Task CreateInviteAsync_CreatesInviteWithCorrectProperties()
        {
            // Arrange
            using var db = GetDbContext(nameof(CreateInviteAsync_CreatesInviteWithCorrectProperties));
            var service = new InviteService(db);
            var email = "test@example.com";

            // Act
            var link = await service.CreateInviteAsync(email);
            var token = await db.InviteTokens.FirstOrDefaultAsync();

            // Assert
            Assert.Contains("signup?token=", link);
            Assert.NotNull(token);
            Assert.Equal(email, token.Email);
            Assert.False(token.Used);
            Assert.InRange(token.ExpiresAt.Value, DateTime.UtcNow.AddDays(6), DateTime.UtcNow.AddDays(8));
            Assert.Contains(token.Token, link);
        }

        [Fact]
        public async Task ValidateInviteAsync_PassesForValidToken()
        {
            // Arrange
            using var db = GetDbContext(nameof(ValidateInviteAsync_PassesForValidToken));
            var service = new InviteService(db);

            var invite = new InviteToken
            {
                Id = Guid.NewGuid(),
                Token = "valid-token",
                Email = "a@b.com",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                Used = false
            };
            db.InviteTokens.Add(invite);
            await db.SaveChangesAsync();

            // Act
            var exception = await Record.ExceptionAsync(() => service.ValidateInviteAsync("valid-token"));

            // Assert
            Assert.Null(exception);
        }

        [Theory]
        [InlineData("expired", true, -1)]  // expiré
        [InlineData("used", true, 1)]      // utilisé
        [InlineData("missing", false, 1)]  // non existant
        public async Task ValidateInviteAsync_Throws_ForInvalidTokens(string tokenValue, bool used, int daysOffset)
        {
            // Arrange
            using var db = GetDbContext(nameof(ValidateInviteAsync_Throws_ForInvalidTokens));
            var service = new InviteService(db);

            if (tokenValue != "missing")
            {
                db.InviteTokens.Add(new InviteToken
                {
                    Id = Guid.NewGuid(),
                    Token = tokenValue,
                    Email = "a@b.com",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(daysOffset > 0 ? daysOffset : -1),
                    Used = used
                });
                await db.SaveChangesAsync();
            }
            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ValidateInviteAsync(tokenValue));
        }

        [Fact]
        public async Task UseInviteAsync_MarksTokenAsUsed()
        {
            // Arrange
            using var db = GetDbContext(nameof(UseInviteAsync_MarksTokenAsUsed));
            var service = new InviteService(db);

            var invite = new InviteToken
            {
                Id = Guid.NewGuid(),
                Token = "use-token",
                Email = "b@b.com",
                CreatedAt = DateTime.UtcNow,
                Used = false
            };
            db.InviteTokens.Add(invite);     
            await db.SaveChangesAsync();

            // Act
            await service.UseInviteAsync(invite.Token);

            var updated = await db.InviteTokens.FindAsync(invite.Id);

            // Assert
            Assert.True(updated.Used);
        }

        [Fact]
        public async Task UseInviteAsync_Throws_WhenTokenAlreadyUsedOrMissing()
        {
            // Arrange
            using var db = GetDbContext(nameof(UseInviteAsync_Throws_WhenTokenAlreadyUsedOrMissing));
            var service = new InviteService(db);

            var invite = new InviteToken
            {
                Id = Guid.NewGuid(),
                Token = "used-token",
                Email = "c@c.com",
                CreatedAt = DateTime.UtcNow,
                Used = true
            };
            db.InviteTokens.Add(invite);
            await db.SaveChangesAsync();

            // Act & Assert
            // déjà utilisé
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.UseInviteAsync(invite.Token));

            // non existant
            Guid newGuid = Guid.NewGuid();
            string token = newGuid.ToString();
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.UseInviteAsync(token));
        }
    }
}
