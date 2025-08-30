using ArzenalStoreApi.Data;
using ArzenalStoreApi.Models;
using ArzenalStoreApi.Services;
using ArzenalStoreApi.Services.Token;
using ArzenalStoreSharedDto.DTOs.AuthDto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace TestArzenalStoreApi.Unit.Services;
public class TokenServiceTests
{
    private AuthDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new AuthDbContext(options);
    }

    private IConfiguration GetConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string> {
            {"Jwt:SecretKey", "supersecretkeysupersecretkeysupersecretkey"},
            {"Jwt:ExpiryHours", "1"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"}
        };
        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public async Task GenerateRefreshToken_CreatesToken()
    {
        var db = GetDbContext(nameof(GenerateRefreshToken_CreatesToken));

        // Crée un utilisateur existant
        var userId = Guid.NewGuid();
        db.Users.Add(new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "fakehash"
        });
        await db.SaveChangesAsync();

        var service = new RefreshTokenService(db);

        var dto = new CreateRefreshTokenDto
        {
            UserId = userId,           // Utilise l'ID de l'utilisateur ajouté
            DeviceName = "TestDevice",
            Fingerprint = "abc123"
        };

        var token = await service.GenerateRefreshToken(dto);

        Assert.False(string.IsNullOrEmpty(token));

        var tokenInDb = await db.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == userId);
        Assert.NotNull(tokenInDb);      // Vérifie qu'il est bien en base
        Assert.Equal(token, tokenInDb.Token);
    }


    [Fact]
    public async Task RotateRefreshTokenAsync_ReturnsUnauthorized_IfTokenNotFound()
    {
        var db = GetDbContext(nameof(RotateRefreshTokenAsync_ReturnsUnauthorized_IfTokenNotFound));
        var service = new RefreshTokenService(db);

        var result = await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await service.RotateRefreshTokenAsync("notfound", "127.0.0.1", "fp");
        });
        Assert.Equal("Refresh token invalide ou expiré.", result.Message);
    }

    [Fact]
    public async Task RotateRefreshTokenAsync_ReturnsUnauthorized_IfFingerprintMismatch()
    {
        var db = GetDbContext(nameof(RotateRefreshTokenAsync_ReturnsUnauthorized_IfFingerprintMismatch));
        var token = new RefreshToken
        {
            Token = "token1",
            Fingerprint = "fp1",
            UserId = Guid.NewGuid(),
            DeviceName = "dev",
            CreatedAt = DateTime.UtcNow
        };
        db.RefreshTokens.Add(token);
        db.SaveChanges();
        var service = new RefreshTokenService(db);

        var result = await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await service.RotateRefreshTokenAsync("token1", "127.0.0.1", "wrongfp");
        });
        Assert.Equal("Fingerprint invalide.", result.Message);
    }

    [Fact]
    public async Task RotateRefreshTokenAsync_UpdatesToken_IfValid()
    {
        var db = GetDbContext(nameof(RotateRefreshTokenAsync_UpdatesToken_IfValid));
        var token = new RefreshToken
        {
            Token = "token2",
            Fingerprint = "fp2",
            UserId = Guid.NewGuid(),
            DeviceName = "dev",
            CreatedAt = DateTime.UtcNow
        };
        db.RefreshTokens.Add(token);
        db.SaveChanges();
        var service = new RefreshTokenService(db);

        var result = await service.RotateRefreshTokenAsync("token2", "127.0.0.1", "fp2");

        Assert.NotNull(result);
        Assert.NotEqual("token2", result.Token);
        Assert.Equal("127.0.0.1", result.CreatedByIp);
        Assert.False(result.IsRevoked);
        Assert.NotNull(result.ExpiresAt);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_ThrowsUnauthorizedAccessException_IfInvalidInput()
    {
        // Arrange
        var db = GetDbContext(nameof(RevokeRefreshTokenAsync_ThrowsUnauthorizedAccessException_IfInvalidInput));
        var service = new RefreshTokenService(db);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await service.RevokeRefreshTokenAsync("notfound", "notfound");
        });

        Assert.Equal("Token ou identifiant utilisateur invalide.", ex.Message);
    }


    [Fact]
    public async Task RevokeRefreshTokenAsync_RevokesToken_IfFound()
    {
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
        var service = new RefreshTokenService(db);
        var result = await service.RevokeRefreshTokenAsync("token3", userId.ToString());

        Assert.True(result);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_ReturnsFalse_IfNotFound()
    {
        var db = GetDbContext(nameof(ValidateRefreshTokenAsync_ReturnsFalse_IfNotFound));
        var service = new RefreshTokenService(db);

        var result = await service.ValidateRefreshTokenAsync("notfound", "fp");
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_ReturnsFalse_IfRevoke()
    {
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
        var service = new RefreshTokenService(db);

        var result = await service.ValidateRefreshTokenAsync("token4", "fp4");
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_ReturnsFalse_IfFingerprintMismatch()
    {
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
        var service = new RefreshTokenService(db);

        var result = await service.ValidateRefreshTokenAsync("token5", "wrongfp");
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_ReturnsTrue_IfValid()
    {
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
        var service = new RefreshTokenService(db);

        var result = await service.ValidateRefreshTokenAsync("token6", "fp6");
        Assert.True(result);
    }

    [Fact]
    public async Task GenerateJwtTokenAsync_ReturnsNull_IfUserNotFound()
    {
        var db = GetDbContext(nameof(GenerateJwtTokenAsync_ReturnsNull_IfUserNotFound));
        var service = new JwtService(db,GetConfiguration());

        var result = await service.GenerateJwtTokenAsync("notfound@email.com");
        Assert.Equal("Utilisateur non trouvé", result);
    }

    [Fact]
    public async Task GenerateJwtTokenAsync_ReturnsToken_IfUserFound()
    {
        var db = GetDbContext(nameof(GenerateJwtTokenAsync_ReturnsToken_IfUserFound));
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@email.com",
            Username = "user",
            PasswordHash = "hash"
        };
        db.Users.Add(user);
        db.SaveChanges();
        var service = new JwtService(db, GetConfiguration());

        var result = await service.GenerateJwtTokenAsync("user@email.com");
        Assert.False(string.IsNullOrEmpty(result));
        // Optionnel : valider le format JWT
        Assert.Contains(".", result);
    }
}
