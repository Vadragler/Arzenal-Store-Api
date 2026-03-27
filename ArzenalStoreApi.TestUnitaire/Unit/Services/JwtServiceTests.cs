using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arzenal.Store.Api.Service.Services.Auth.Tokens;
using Arzenal.Store.Api.Infrastructure.Data;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace ArzenalStoreApi.TestUnitaire.Unit.Services
{
    public class JwtServiceTests
    {
        private static AuthDbContext CreateInMemoryAuthDb(string dbName)
        {
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new AuthDbContext(options);
        }

        [Fact]
        public async Task GenerateJwtTokenAsync_ThrowsNotFound_WhenUserNotExists()
        {
            // Arrange
            await using var db = CreateInMemoryAuthDb("jwt_test_missing_user");
            var inMemory = new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "test-secret-key-1234567890",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:ExpiryHours"] = "1"
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemory).Build();
            var cookieServiceMock = new Mock<ICookieService>();
            var service = new JwtService(db, configuration, cookieServiceMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => service.GenerateJwtTokenAsync(Guid.NewGuid(), new List<string>{"web"}));
        }

        [Fact]
        public async Task GenerateJwtTokenAsync_ReturnsTokenWithAudiences_WhenValid()
        {
            // Arrange
            var dbName = "jwt_test_valid_token" + Guid.NewGuid();
            await using var db = CreateInMemoryAuthDb(dbName);
            var userId = Guid.NewGuid();
            db.Users.Add(new Arzenal.Store.Api.Domain.Models.User { Id = userId, PasswordHash = "hashed-password", Username = "testuser", Email = "a@b.com", Role = "Admin" });
            await db.SaveChangesAsync();

            var inMemory = new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "test-secret-key-123456789012345678",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:ExpiryHours"] = "1"
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemory).Build();
            var cookieServiceMock = new Mock<ICookieService>();
            var service = new JwtService(db, configuration, cookieServiceMock.Object);

            // Act
            var token = await service.GenerateJwtTokenAsync(userId, new List<string> { "web", "ArzenalStoreManager" });

            // Assert
            Assert.False(string.IsNullOrEmpty(token));
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var audClaims = jwt.Claims.Where(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Aud).Select(c => c.Value).ToList();
            Assert.Contains("web", audClaims);
            Assert.Contains("ArzenalStoreManager", audClaims);
        }

        [Fact]
        public async Task GenerateJwtTokenForAppAsync_Throws_WhenMissingXAppId()
        {
            // Arrange
            await using var db = CreateInMemoryAuthDb("jwt_test_missing_xappid");
            var inMemory = new Dictionary<string, string?>
            {
                ["Jwt-App-Login:SecretKey"] = "app-secret-key-123456",
                ["Jwt-App-Login:Issuer"] = "app-issuer"
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemory).Build();
            var cookieServiceMock = new Mock<ICookieService>();
            var service = new JwtService(db, configuration, cookieServiceMock.Object);

            var context = new DefaultHttpContext();
            var request = context.Request;

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GenerateJwtTokenForAppAsync(request));
        }
    }
}
