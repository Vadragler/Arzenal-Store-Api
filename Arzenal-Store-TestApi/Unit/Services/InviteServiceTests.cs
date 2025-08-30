using ArzenalStoreApi.Data;
using ArzenalStoreApi.Models;
using ArzenalStoreApi.Services.InviteService;
using Microsoft.EntityFrameworkCore;


namespace TestArzenalStoreApi.Unit.Services
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
            using var db = GetDbContext(nameof(CreateInviteAsync_CreatesInviteWithCorrectProperties));
            var service = new InviteService(db);

            var email = "test@example.com";
            var link = await service.CreateInviteAsync(email);

            // Vérifie que le lien contient le token
            Assert.Contains("signup?token=", link);

            // Récupère le token en base
            var token = await db.InviteTokens.FirstOrDefaultAsync();
            Assert.NotNull(token);
            Assert.Equal(email, token.Email);
            Assert.False(token.Used);
            Assert.InRange(token.ExpiresAt.Value, DateTime.UtcNow.AddDays(6), DateTime.UtcNow.AddDays(8));
            Assert.Contains(token.Token, link);
        }

        [Fact]
        public async Task ValidateInviteAsync_PassesForValidToken()
        {
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

            await service.ValidateInviteAsync("valid-token"); // ne doit pas throw
        }

        [Theory]
        [InlineData("expired", true, -1)]  // expiré
        [InlineData("used", true, 1)]      // utilisé
        [InlineData("missing", false, 1)]  // non existant
        public async Task ValidateInviteAsync_Throws_ForInvalidTokens(string tokenValue, bool used, int daysOffset)
        {
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

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ValidateInviteAsync(tokenValue));
        }

        [Fact]
        public async Task UseInviteAsync_MarksTokenAsUsed()
        {
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

            await service.UseInviteAsync(invite.Id);

            var updated = await db.InviteTokens.FindAsync(invite.Id);
            Assert.True(updated.Used);
        }

        [Fact]
        public async Task UseInviteAsync_Throws_WhenTokenAlreadyUsedOrMissing()
        {
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

            // déjà utilisé
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.UseInviteAsync(invite.Id));

            // non existant
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.UseInviteAsync(Guid.NewGuid()));
        }
    }
}
