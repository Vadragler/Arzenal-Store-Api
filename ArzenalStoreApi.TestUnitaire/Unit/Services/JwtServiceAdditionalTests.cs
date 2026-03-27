using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arzenal.Store.Api.Service.Services.Auth.Tokens;
using Arzenal.Store.Api.Infrastructure.Data;
using Arzenal.Store.Api.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Arzenal.Store.Api.Domain.Models.Requests;

namespace ArzenalStoreApi.TestUnitaire.Unit.Services
{
    public class JwtServiceAdditionalTests
    {
        private static AuthDbContext CreateInMemoryAuthDb(string dbName)
        {
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new AuthDbContext(options);
        }

        [Fact]
        public async Task GenerateJwtTokenForAppAsync_ReturnsToken_WhenValid()
        {
            // Arrange
            var dbName = "jwt_test_app_valid" + Guid.NewGuid();
            await using var db = CreateInMemoryAuthDb(dbName);
            var userId = Guid.NewGuid();
            db.Users.Add(new Arzenal.Store.Api.Domain.Models.User { Id = userId, PasswordHash = "hashed-password", Username = "appuser", Email = "app@b.com", Role = "Admin" });
            await db.SaveChangesAsync();

            var inMemory = new Dictionary<string, string?>
            {
                ["Jwt-App-Login:SecretKey"] = "app-secret-key-1234567890123456789",
                ["Jwt-App-Login:Issuer"] = "app-issuer"
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemory).Build();

            var cookieServiceMock = new Mock<ICookieService>();
            cookieServiceMock.Setup(c => c.GetRefreshToken(It.IsAny<HttpRequest>())).Returns(new RefreshTokenCookieData { Token = "rt", UserId = userId });

            var service = new JwtService(db, configuration, cookieServiceMock.Object);

            var context = new DefaultHttpContext();
            context.Request.Headers["X-App-Id"] = "integration-app";

            // Act
            var token = await service.GenerateJwtTokenForAppAsync(context.Request);

            // Assert
            Assert.False(string.IsNullOrEmpty(token));
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            Assert.Contains(jwt.Claims, c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Aud && c.Value == "app-login");
        }

        [Fact]
        public async Task GenerateJwtTokenAsync_Throws_WhenSecretMissing()
        {
            await using var db = CreateInMemoryAuthDb("jwt_test_secret_missing");
            // Add a user so the service reaches the secret check instead of throwing NotFoundException
            var userId = Guid.NewGuid();
            db.Users.Add(new Arzenal.Store.Api.Domain.Models.User { Id = userId, PasswordHash = "hashed", Username = "user", Email = "u@b.com", Role = "User" });
            await db.SaveChangesAsync();

            var inMemory = new Dictionary<string, string?>(); // no Jwt:SecretKey
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemory).Build();
            var cookieServiceMock = new Mock<ICookieService>();
            var service = new JwtService(db, configuration, cookieServiceMock.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GenerateJwtTokenAsync(userId, new List<string>{"web"}));
        }

        [Fact]
        public async Task GenerateJwtTokenForAppAsync_Throws_WhenRefreshTokenUserNotFound()
        {
            // Arrange
            await using var db = CreateInMemoryAuthDb("jwt_test_refresh_user_not_found");
            var inMemory = new Dictionary<string, string?>
            {
                ["Jwt-App-Login:SecretKey"] = "app-secret-key-123456",
                ["Jwt-App-Login:Issuer"] = "app-issuer"
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemory).Build();
            var cookieServiceMock = new Mock<ICookieService>();
            // Return a refresh token referencing a non-existing user
            cookieServiceMock.Setup(c => c.GetRefreshToken(It.IsAny<HttpRequest>())).Returns(new RefreshTokenCookieData { Token = "rt", UserId = Guid.NewGuid() });
            var service = new JwtService(db, configuration, cookieServiceMock.Object);

            var context = new DefaultHttpContext();
            context.Request.Headers["X-App-Id"] = "integration-app";

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GenerateJwtTokenForAppAsync(context.Request));
        }
    }
}
