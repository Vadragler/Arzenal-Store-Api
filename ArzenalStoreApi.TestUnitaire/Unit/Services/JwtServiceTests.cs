using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.Domain.Models.Requests;
using Arzenal.Store.Api.Infrastructure.Data;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Store.Api.Service.Services.Auth.Tokens;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ArzenalStoreApi.TestUnitaire.Unit.Services;

public class JwtServiceTests
{
    private static AuthDbContext CreateContext(string name) => new(
        new DbContextOptionsBuilder<AuthDbContext>().UseInMemoryDatabase(name).Options);

    private static IConfiguration CreateConfiguration() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = "unit-test-jwt-secret-must-be-at-least-thirty-two-characters",
            ["Jwt:Issuer"] = "unit-test-issuer",
            ["Jwt:ExpiryHours"] = "1",
            ["Jwt-App-Login:SecretKey"] = "unit-test-app-jwt-secret-must-be-at-least-thirty-two-characters",
            ["Jwt-App-Login:Issuer"] = "unit-test-app-issuer",
            ["AppLogin:AllowedAppIds:0"] = "test-app"
        })
        .Build();

    private static IRefreshTokenHasher CreateHasher()
    {
        var hasher = new Mock<IRefreshTokenHasher>();
        hasher.Setup(service => service.Hash(It.IsAny<string>()))
            .Returns((string token) => $"hash:{token}");
        return hasher.Object;
    }

    [Fact]
    public async Task GenerateJwtTokenAsync_ThrowsNotFound_WhenUserDoesNotExist()
    {
        await using var db = CreateContext(nameof(GenerateJwtTokenAsync_ThrowsNotFound_WhenUserDoesNotExist));
        var service = new JwtService(db, CreateConfiguration(), Mock.Of<ICookieService>(), CreateHasher());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GenerateJwtTokenAsync(Guid.NewGuid(), ["web"]));
    }

    [Fact]
    public async Task GenerateJwtTokenAsync_ContainsRequestedAudiences()
    {
        await using var db = CreateContext(nameof(GenerateJwtTokenAsync_ContainsRequestedAudiences));
        var userId = Guid.NewGuid();
        db.Users.Add(new User { Id = userId, Username = "user", Email = "user@example.com", PasswordHash = "hash", Role = "Admin" });
        await db.SaveChangesAsync();
        var service = new JwtService(db, CreateConfiguration(), Mock.Of<ICookieService>(), CreateHasher());

        var token = await service.GenerateJwtTokenAsync(userId, ["web", "ArzenalStoreManager"]);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Contains(jwt.Audiences, audience => audience == "web");
        Assert.Contains(jwt.Audiences, audience => audience == "ArzenalStoreManager");
    }

    [Fact]
    public async Task GenerateJwtTokenForApp_RequiresAnAllowedAppAndTheUsersOwnActiveRefreshToken()
    {
        await using var db = CreateContext(nameof(GenerateJwtTokenForApp_RequiresAnAllowedAppAndTheUsersOwnActiveRefreshToken));
        var userId = Guid.NewGuid();
        db.Users.Add(new User { Id = userId, Username = "user", Email = "user@example.com", PasswordHash = "hash", Role = "Admin" });
        db.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = "hash:refresh",
            UserId = userId,
            FamilyId = Guid.NewGuid(),
            DeviceName = "device",
            Fingerprint = "fp",
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        });
        await db.SaveChangesAsync();

        var cookies = new Mock<ICookieService>();
        cookies.Setup(service => service.GetRefreshToken(It.IsAny<HttpRequest>()))
            .Returns(new RefreshTokenCookieData { Token = "refresh", UserId = Guid.NewGuid() });
        var service = new JwtService(db, CreateConfiguration(), cookies.Object, CreateHasher());
        var context = new DefaultHttpContext();
        context.Request.Headers["X-App-Id"] = "test-app";

        var token = await service.GenerateJwtTokenForAppAsync(userId, context.Request);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Contains(jwt.Claims, claim => claim.Type == JwtRegisteredClaimNames.NameId && claim.Value == userId.ToString());
        Assert.Contains(jwt.Claims, claim => claim.Type == "appId" && claim.Value == "test-app");
    }

    [Fact]
    public async Task GenerateJwtTokenForApp_RejectsUnknownAppAndOtherUsersRefreshToken()
    {
        await using var db = CreateContext(nameof(GenerateJwtTokenForApp_RejectsUnknownAppAndOtherUsersRefreshToken));
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        db.Users.AddRange(
            new User { Id = userId, Username = "user", Email = "user@example.com", PasswordHash = "hash" },
            new User { Id = otherUserId, Username = "other", Email = "other@example.com", PasswordHash = "hash" });
        db.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = "hash:refresh",
            UserId = otherUserId,
            FamilyId = Guid.NewGuid(),
            DeviceName = "device",
            Fingerprint = "fp",
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        });
        await db.SaveChangesAsync();

        var cookies = new Mock<ICookieService>();
        cookies.Setup(service => service.GetRefreshToken(It.IsAny<HttpRequest>()))
            .Returns(new RefreshTokenCookieData { Token = "refresh" });
        var service = new JwtService(db, CreateConfiguration(), cookies.Object, CreateHasher());
        var context = new DefaultHttpContext();
        context.Request.Headers["X-App-Id"] = "unknown-app";

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.GenerateJwtTokenForAppAsync(userId, context.Request));

        context.Request.Headers["X-App-Id"] = "test-app";
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.GenerateJwtTokenForAppAsync(userId, context.Request));
    }
}
