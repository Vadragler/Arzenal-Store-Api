using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.Domain.Models.Requests;
using Arzenal.Store.Api.Infrastructure.Data;
using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Store.Api.Service.Models.Auth;
using Arzenal.Store.Api.Service.Services.Auth.Tokens;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Cryptography;
using System.Text;

namespace ArzenalStoreApi.TestUnitaire.Unit.Services;

public class TokenServiceTests
{
    private static AuthDbContext CreateContext(string name) => new(
        new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(name)
            .Options);

    private static IRefreshTokenHasher CreateHasher()
    {
        var hasher = new Mock<IRefreshTokenHasher>();
        hasher.Setup(service => service.Hash(It.IsAny<string>()))
            .Returns((string token) => Hash(token));
        return hasher.Object;
    }

    private static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static RefreshToken ExistingToken(string rawToken, Guid userId, string fingerprint, Guid? familyId = null) => new()
    {
        TokenHash = Hash(rawToken),
        UserId = userId,
        FamilyId = familyId ?? Guid.NewGuid(),
        Fingerprint = fingerprint,
        DeviceName = "device",
        ExpiresAt = DateTime.UtcNow.AddDays(30),
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        LastUsed = DateTime.UtcNow
    };

    private static DefaultHttpContext CreateRequest(string fingerprint)
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["Fingerprint"] = fingerprint;
        return context;
    }

    [Fact]
    public async Task GenerateRefreshToken_StoresOnlyTheHmacHash()
    {
        await using var db = CreateContext(nameof(GenerateRefreshToken_StoresOnlyTheHmacHash));
        var userId = Guid.NewGuid();
        db.Users.Add(new User { Id = userId, Username = "user", Email = "user@example.com", PasswordHash = "hash" });
        await db.SaveChangesAsync();

        var service = new RefreshTokenService(db, Mock.Of<ICookieService>(), CreateHasher());
        var cookieData = await service.GenerateRefreshToken(new RefreshTokenCreationRequest(
            userId, "device", "fingerprint", "agent", "127.0.0.1"));

        var stored = await db.RefreshTokens.SingleAsync();
        Assert.NotEmpty(cookieData.Token);
        Assert.Equal(Hash(cookieData.Token), stored.TokenHash);
        Assert.NotEqual(cookieData.Token, stored.TokenHash);
    }

    [Fact]
    public async Task RotateRefreshToken_RotatesTheTokenAndKeepsItsFamily()
    {
        await using var db = CreateContext(nameof(RotateRefreshToken_RotatesTheTokenAndKeepsItsFamily));
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var original = ExistingToken("old", userId, "fp", familyId);
        db.RefreshTokens.Add(original);
        await db.SaveChangesAsync();

        var cookies = new Mock<ICookieService>();
        cookies.Setup(service => service.GetRefreshToken(It.IsAny<HttpRequest>()))
            .Returns(new RefreshTokenCookieData { Token = "old" });
        var service = new RefreshTokenService(db, cookies.Object, CreateHasher());

        var result = await service.RotateRefreshTokenAsync(CreateRequest("fp"));

        var tokens = await db.RefreshTokens.OrderBy(token => token.CreatedAt).ToListAsync();
        Assert.NotNull(result);
        Assert.Equal(2, tokens.Count);
        Assert.True(tokens.Single(token => token.Id == original.Id).IsRevoked);
        Assert.Equal(Hash(result!.Token), tokens.Single(token => !token.IsRevoked).TokenHash);
        Assert.All(tokens, token => Assert.Equal(familyId, token.FamilyId));
    }

    [Fact]
    public async Task RotateRefreshToken_ReusedOldToken_RevokesTheEntireFamily()
    {
        await using var db = CreateContext(nameof(RotateRefreshToken_ReusedOldToken_RevokesTheEntireFamily));
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        db.RefreshTokens.Add(ExistingToken("old", userId, "fp", familyId));
        await db.SaveChangesAsync();

        var cookies = new Mock<ICookieService>();
        cookies.Setup(service => service.GetRefreshToken(It.IsAny<HttpRequest>()))
            .Returns(new RefreshTokenCookieData { Token = "old" });
        var service = new RefreshTokenService(db, cookies.Object, CreateHasher());
        var context = CreateRequest("fp");

        await service.RotateRefreshTokenAsync(context);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.RotateRefreshTokenAsync(context));

        var tokens = await db.RefreshTokens.Where(token => token.FamilyId == familyId).ToListAsync();
        Assert.NotEmpty(tokens);
        Assert.All(tokens, token => Assert.True(token.IsRevoked));
    }

    [Fact]
    public async Task RotateRefreshToken_FingerprintMismatch_RevokesTheFamily()
    {
        await using var db = CreateContext(nameof(RotateRefreshToken_FingerprintMismatch_RevokesTheFamily));
        var userId = Guid.NewGuid();
        var token = ExistingToken("current", userId, "expected");
        db.RefreshTokens.Add(token);
        await db.SaveChangesAsync();

        var cookies = new Mock<ICookieService>();
        cookies.Setup(service => service.GetRefreshToken(It.IsAny<HttpRequest>()))
            .Returns(new RefreshTokenCookieData { Token = "current" });
        var service = new RefreshTokenService(db, cookies.Object, CreateHasher());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.RotateRefreshTokenAsync(CreateRequest("other")));

        Assert.True((await db.RefreshTokens.SingleAsync()).IsRevoked);
    }

    [Fact]
    public async Task RotateRefreshToken_RejectsAFamilyPastItsAbsoluteExpiration()
    {
        await using var db = CreateContext(nameof(RotateRefreshToken_RejectsAFamilyPastItsAbsoluteExpiration));
        var userId = Guid.NewGuid();
        var token = ExistingToken("current", userId, "fp");
        token.AbsoluteExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        db.RefreshTokens.Add(token);
        await db.SaveChangesAsync();

        var cookies = new Mock<ICookieService>();
        cookies.Setup(service => service.GetRefreshToken(It.IsAny<HttpRequest>()))
            .Returns(new RefreshTokenCookieData { Token = "current" });
        var service = new RefreshTokenService(db, cookies.Object, CreateHasher());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.RotateRefreshTokenAsync(CreateRequest("fp")));
        Assert.True((await db.RefreshTokens.SingleAsync()).IsRevoked);
    }

    [Fact]
    public async Task RotateRefreshToken_UnknownToken_IsRejected()
    {
        await using var db = CreateContext(nameof(RotateRefreshToken_UnknownToken_IsRejected));
        var cookies = new Mock<ICookieService>();
        cookies.Setup(service => service.GetRefreshToken(It.IsAny<HttpRequest>()))
            .Returns(new RefreshTokenCookieData { Token = "unknown" });
        var service = new RefreshTokenService(db, cookies.Object, CreateHasher());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.RotateRefreshTokenAsync(CreateRequest("fp")));
    }

    [Fact]
    public async Task RevokeRefreshToken_UsesTheHashToFindTheStoredToken()
    {
        await using var db = CreateContext(nameof(RevokeRefreshToken_UsesTheHashToFindTheStoredToken));
        var userId = Guid.NewGuid();
        db.RefreshTokens.Add(ExistingToken("raw", userId, "fp"));
        await db.SaveChangesAsync();
        var service = new RefreshTokenService(db, Mock.Of<ICookieService>(), CreateHasher());

        var revoked = await service.RevokeRefreshTokenAsync("raw", userId);

        Assert.True(revoked);
        Assert.True((await db.RefreshTokens.SingleAsync()).IsRevoked);
    }

    [Fact]
    public async Task ValidateRefreshToken_RequiresAnActiveMatchingFingerprint()
    {
        await using var db = CreateContext(nameof(ValidateRefreshToken_RequiresAnActiveMatchingFingerprint));
        var userId = Guid.NewGuid();
        db.RefreshTokens.Add(ExistingToken("raw", userId, "fp"));
        await db.SaveChangesAsync();
        var service = new RefreshTokenService(db, Mock.Of<ICookieService>(), CreateHasher());

        Assert.True(await service.ValidateRefreshTokenAsync("raw", "fp"));
        Assert.False(await service.ValidateRefreshTokenAsync("raw", "other"));
        Assert.False(await service.ValidateRefreshTokenAsync("unknown", "fp"));
    }
}
