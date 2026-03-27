using Arzenal.Store.Api.Domain.Models.Requests;
using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Services.Auth.Cookies;
using Arzenal.Store.Api.Service.Services.Auth.Tokens;
using Arzenal.Dto.DTOs.AuthDto;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;
using Arzenal.Store.Api.Infrastructure.Data;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Services;
public class TokenServiceTests
{
    private AuthDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new AuthDbContext(options);
    }

    private static IConfiguration GetConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string> {
            {"Jwt:SecretKey", "supersecretkeysupersecretkeysupersecretkey"},
            {"Jwt:ExpiryHours", "1"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"}
        };
        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();
    }

    [Fact]
    public async Task GenerateRefreshToken_CreatesToken()
    {
        // Arrange
        var db = GetDbContext(nameof(GenerateRefreshToken_CreatesToken));

        var userId = Guid.NewGuid();
        db.Users.Add(new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "fakehash"
        });
        await db.SaveChangesAsync();
        var service = new RefreshTokenService(db, new CookieService(GetConfiguration()));

        var dto = new CreateRefreshTokenDto
        {
            UserId = userId,
            DeviceName = "TestDevice",
            Fingerprint = "abc123"
        };

        // Act
        var token = await service.GenerateRefreshToken(dto);
        var tokenInDb = await db.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == userId);

        // Assert
        Assert.False(string.IsNullOrEmpty(token.Token));
        Assert.NotNull(tokenInDb);
        Assert.Equal(token.Token, tokenInDb.Token);
    }


    [Fact]
    public async Task RotateRefreshTokenAsync_ReturnsUnauthorized_IfTokenNotFound()
    {
        // Arrange
        var db = GetDbContext(nameof(RotateRefreshTokenAsync_ReturnsUnauthorized_IfTokenNotFound));

        var cookieMock = new Mock<ICookieService>();
        cookieMock.Setup(c => c.GetRefreshToken(It.IsAny<HttpRequest>()))
            .Returns(new RefreshTokenCookieData
            {
                Token = "inexistant_token",
                UserId = Guid.NewGuid()
            });
        var service = new RefreshTokenService(db, cookieMock.Object);

        var context = new DefaultHttpContext();
        context.Request.Headers["Fingerprint"] = "test-fp";
        context.Request.Headers["x-Device-Name"] = "test-device";
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
        new Claim("UserId", Guid.NewGuid().ToString())
    }));

        // Act & Assert
        var result = await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await service.RotateRefreshTokenAsync(context);
        });

        Assert.Equal("Session expirée ou invalide. Veuillez vous reconnecter.", result.Message);
    }


    [Fact]
    public async Task RotateRefreshTokenAsync_ReturnsUnauthorized_IfFingerprintMismatch()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var db = GetDbContext(nameof(RotateRefreshTokenAsync_ReturnsUnauthorized_IfFingerprintMismatch));

        var token = new RefreshToken
        {
            Token = "token1",
            Fingerprint = "fp1",
            UserId = userId,
            DeviceName = "dev",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        db.RefreshTokens.Add(token);
        db.SaveChanges();

        var cookieMock = new Mock<ICookieService>();
        cookieMock.Setup(c => c.GetRefreshToken(It.IsAny<HttpRequest>()))
            .Returns(new RefreshTokenCookieData
            {
                Token = "token1",
                UserId = userId
            });
        var service = new RefreshTokenService(db, cookieMock.Object);

        var context = new DefaultHttpContext();
        context.Request.Headers["Fingerprint"] = "wrongfp";
        context.Request.Headers["Device-Name"] = "dev";
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("UserId", userId.ToString())
        }));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.RotateRefreshTokenAsync(context));

        Assert.Equal("Fingerprint invalide.", ex.Message);
    }

    [Fact]
    public async Task RotateRefreshTokenAsync_ThrowsUnauthorized_WhenNoCookie()
    {
        // Arrange
        var db = GetDbContext(nameof(RotateRefreshTokenAsync_ThrowsUnauthorized_WhenNoCookie));
        var cookieMock = new Mock<ICookieService>();
        cookieMock.Setup(c => c.GetRefreshToken(It.IsAny<HttpRequest>()))
            .Returns((RefreshTokenCookieData?)null);

        var service = new RefreshTokenService(db, cookieMock.Object);

        var context = new DefaultHttpContext();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.RotateRefreshTokenAsync(context));

        Assert.Equal("Aucun token de rafraîchissement fourni.", ex.Message);
    }

    [Fact]
    public async Task RotateRefreshTokenAsync_UpdatesToken_IfValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var db = GetDbContext(nameof(RotateRefreshTokenAsync_UpdatesToken_IfValid));

        var token = new RefreshToken
        {
            Token = "old_token",
            Fingerprint = "fp2",
            UserId = userId,
            DeviceName = "dev",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        db.RefreshTokens.Add(token);
        db.SaveChanges();

        var cookieMock = new Mock<ICookieService>();
        cookieMock.Setup(c => c.GetRefreshToken(It.IsAny<HttpRequest>()))
            .Returns(new RefreshTokenCookieData
            {
                Token = "old_token",
                UserId = userId
            });

        var service = new RefreshTokenService(db, cookieMock.Object);

        var context = new DefaultHttpContext();
        context.Request.Headers["Fingerprint"] = "fp2";
        context.Request.Headers["Device-Name"] = "dev";
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("UserId", userId.ToString())
        }));

        // Act
        var result = await service.RotateRefreshTokenAsync(context);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual("old_token", result.Token);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_ReturnsFalse_IfInvalidInput()
    {
        // Arrange
        var db = GetDbContext(nameof(RevokeRefreshTokenAsync_ReturnsFalse_IfInvalidInput));
        var service = new RefreshTokenService(db, new CookieService(GetConfiguration()));

        // Act
        var result = await service.RevokeRefreshTokenAsync("notfound", Guid.NewGuid());

        // Assert
        Assert.False(result);
    }


    [Fact]
    public async Task RevokeRefreshTokenAsync_RevokesToken_IfFound()
    {
        // Arrange
        var db = GetDbContext(nameof(RevokeRefreshTokenAsync_RevokesToken_IfFound));
        var userId = Guid.NewGuid();
        var token = new RefreshToken
        {
            Token = "token3",
            IsRevoked = false,
            Fingerprint = "fp3",
            UserId = userId,
            DeviceName = "dev",
            CreatedAt = DateTime.UtcNow
        };
        db.RefreshTokens.Add(token);
        db.SaveChanges();
        var service = new RefreshTokenService(db, new CookieService(GetConfiguration()));

        // Act
        var result = await service.RevokeRefreshTokenAsync("token3", userId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_ReturnsFalse_IfNotFound()
    {
        // Arrange
        var db = GetDbContext(nameof(ValidateRefreshTokenAsync_ReturnsFalse_IfNotFound));
        var service = new RefreshTokenService(db, new CookieService(GetConfiguration()));

        // Act
        var result = await service.ValidateRefreshTokenAsync("notfound", "fp");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_ReturnsFalse_IfRevoke()
    {
        // Arrange
        var db = GetDbContext(nameof(ValidateRefreshTokenAsync_ReturnsFalse_IfRevoke));
        var token = new RefreshToken
        {
            Token = "token4",
            Fingerprint = "fp4",
            UserId = Guid.NewGuid(),
            DeviceName = "dev",
            CreatedAt = DateTime.UtcNow,
            IsRevoked = true
        };
        db.RefreshTokens.Add(token);
        db.SaveChanges();
        var service = new RefreshTokenService(db, new CookieService(GetConfiguration()));

        // Act
        var result = await service.ValidateRefreshTokenAsync("token4", "fp4");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_ReturnsFalse_IfFingerprintMismatch()
    {
        // Arrange
        var db = GetDbContext(nameof(ValidateRefreshTokenAsync_ReturnsFalse_IfFingerprintMismatch));
        var token = new RefreshToken
        {
            Token = "token5",
            Fingerprint = "fp5",
            UserId = Guid.NewGuid(),
            DeviceName = "dev",
            CreatedAt = DateTime.UtcNow
        };
        db.RefreshTokens.Add(token);
        db.SaveChanges();
        var service = new RefreshTokenService(db, new CookieService(GetConfiguration()));

        // Act
        var result = await service.ValidateRefreshTokenAsync("token5", "wrongfp");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_ReturnsTrue_IfValid()
    {
        // Arrange
        var db = GetDbContext(nameof(ValidateRefreshTokenAsync_ReturnsTrue_IfValid));
        var token = new RefreshToken
        {
            Token = "token6",
            Fingerprint = "fp6",
            UserId = Guid.NewGuid(),
            DeviceName = "dev",
            CreatedAt = DateTime.UtcNow
        };
        db.RefreshTokens.Add(token);
        db.SaveChanges();
        var service = new RefreshTokenService(db, new CookieService(GetConfiguration()));

        // Act
        var result = await service.ValidateRefreshTokenAsync("token6", "fp6");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GenerateJwtTokenAsync_ThrowNotFound_IfUserNotFound()
    {
        // Arrange
        var cookieService = new CookieService(GetConfiguration());
        var db = GetDbContext(nameof(GenerateJwtTokenAsync_ThrowNotFound_IfUserNotFound));
        var service = new JwtService(db, GetConfiguration(), cookieService);
        List<string> audiences = new List<string> { "ArzenalAuth" };
        // Act & Assert
        var result = await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await service.GenerateJwtTokenAsync(Guid.NewGuid(), audiences);
        });
    }

    [Fact]
    public async Task GenerateJwtTokenAsync_ReturnsToken_IfUserFound()
    {
        // Arrange
        var cookieService = new CookieService(GetConfiguration());
        var db = GetDbContext(nameof(GenerateJwtTokenAsync_ReturnsToken_IfUserFound));
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "user@email.com",
            Username = "user",
            PasswordHash = "hash"
        };
        db.Users.Add(user);
        db.SaveChanges();
        var service = new JwtService(db, GetConfiguration(), cookieService);
        List<string> audiences = new List<string> { "ArzenalAuth" };
        // Act
        var result = await service.GenerateJwtTokenAsync(userId, audiences);

        // Assert
        Assert.False(string.IsNullOrEmpty(result));
        Assert.Contains(".", result);
    }


}
