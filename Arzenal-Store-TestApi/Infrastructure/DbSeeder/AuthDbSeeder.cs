using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.Service.Services.Auth.Passwords;
using Arzenal.Store.Api.Infrastructure.Data;

namespace Arzenal.Store.Api.TestIntegration.Infrastructure.DbSeeder
{
    public static class AuthDbSeeder
    {
        public static void Seed(AuthDbContext db)
        {
            var now = DateTime.UtcNow;
            var passwordService = new PasswordService();
            var user = new User
            {
                Id = Guid.Parse("d3f1a9e2-5b6c-4f7d-9a12-8c3e4b5f6789"),
                Username = "existinguser",
                Email = "existing@example.com",
                Role = "Admin",
                PasswordHash = passwordService.Hash("hashed-password")
            };
            db.Users.Add(user);
            db.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.Parse("a1b2c3d4-e5f6-4789-9012-3456789abcde"),
                UserId = user.Id,
                Token = "valid-refresh-token",
                Fingerprint = "Fingerprint",
                CreatedByIp = "127.0.0.1",
                DeviceName = "TEST-DEVICE",
                ExpiresAt = now.AddDays(30),
                CreatedAt = now,
                IsRevoked = false,
                Type = "Chrome"
            });
            db.InviteTokens.Add(new InviteToken
            {
                Email = "test@exemple.com",
                Token = "valid-token",
                ExpiresAt = now.AddHours(1),
                Used = false
            });
            db.SaveChanges();
        }
    }

}