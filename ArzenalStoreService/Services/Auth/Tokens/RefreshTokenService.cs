using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.Domain.Models.Requests;
using Arzenal.Store.Api.Infrastructure.Data;
using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Store.Api.Service.Models.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Arzenal.Store.Api.Service.Services.Auth.Tokens
{
    public class RefreshTokenService(
        AuthDbContext context,
        ICookieService cookieService,
        IRefreshTokenHasher refreshTokenHasher) : IRefreshTokenService
    {
        private readonly AuthDbContext _dbContext = context;
        private readonly ICookieService _cookieService = cookieService;
        private readonly IRefreshTokenHasher _refreshTokenHasher = refreshTokenHasher;

        public async Task<RefreshTokenCookieData> GenerateRefreshToken(RefreshTokenCreationRequest request)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId)
                ?? throw new InvalidOperationException($"Utilisateur non trouvé. {request.UserId}");

            var now = DateTime.UtcNow;
            var rawToken = GenerateSecureRefreshToken();
            var tokenHash = _refreshTokenHasher.Hash(rawToken);

            var previousTokens = await _dbContext.RefreshTokens
                .Where(token => token.UserId == request.UserId &&
                                token.DeviceName == request.DeviceName &&
                                !token.IsRevoked)
                .ToListAsync();

            foreach (var previousToken in previousTokens)
                Revoke(previousToken, now, tokenHash);

            _dbContext.RefreshTokens.Add(CreateToken(request, tokenHash, Guid.NewGuid(), now));
            await _dbContext.SaveChangesAsync();

            return new RefreshTokenCookieData { Token = rawToken, UserId = user.Id };
        }

        public async Task<RefreshTokenCookieData?> RotateRefreshTokenAsync(HttpContext context)
        {
            var cookie = _cookieService.GetRefreshToken(context.Request);
            if (cookie == null || string.IsNullOrWhiteSpace(cookie.Token))
                throw new UnauthorizedAccessException("Aucun token de rafraîchissement fourni.");

            var now = DateTime.UtcNow;
            var currentTokenHash = _refreshTokenHasher.Hash(cookie.Token);
            var currentToken = await _dbContext.RefreshTokens
                .SingleOrDefaultAsync(token => token.TokenHash == currentTokenHash);

            if (currentToken == null)
                throw InvalidRefreshToken();

            if (currentToken.IsRevoked)
            {
                await RevokeFamilyAsync(currentToken.FamilyId, now);
                throw InvalidRefreshToken();
            }

            if (currentToken.ExpiresAt is not null && currentToken.ExpiresAt <= now)
            {
                Revoke(currentToken, now);
                await _dbContext.SaveChangesAsync();
                throw InvalidRefreshToken();
            }

            if (currentToken.AbsoluteExpiresAt <= now)
            {
                Revoke(currentToken, now);
                await _dbContext.SaveChangesAsync();
                throw InvalidRefreshToken();
            }

            if (!string.Equals(currentToken.Fingerprint, context.Request.Headers["Fingerprint"], StringComparison.Ordinal))
            {
                await RevokeFamilyAsync(currentToken.FamilyId, now);
                throw new UnauthorizedAccessException("Fingerprint invalide.");
            }

            var newRawToken = GenerateSecureRefreshToken();
            var newTokenHash = _refreshTokenHasher.Hash(newRawToken);
            Revoke(currentToken, now, newTokenHash);
            _dbContext.RefreshTokens.Add(new RefreshToken
            {
                TokenHash = newTokenHash,
                UserId = currentToken.UserId,
                FamilyId = currentToken.FamilyId,
                DeviceName = currentToken.DeviceName,
                Fingerprint = currentToken.Fingerprint,
                UserAgent = context.Request.Headers["User-Agent"].FirstOrDefault(),
                CreatedByIp = context.Connection.RemoteIpAddress?.ToString(),
                Type = currentToken.Type,
                CreatedAt = now,
                UpdatedAt = now,
                LastUsed = now,
                ExpiresAt = Min(now.AddDays(30), currentToken.AbsoluteExpiresAt),
                AbsoluteExpiresAt = currentToken.AbsoluteExpiresAt
            });

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                _dbContext.ChangeTracker.Clear();
                await RevokeFamilyAsync(currentToken.FamilyId, now);
                throw InvalidRefreshToken();
            }

            return new RefreshTokenCookieData { Token = newRawToken, UserId = currentToken.UserId };
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, Guid? userId)
        {
            if (string.IsNullOrWhiteSpace(refreshToken) || userId is null || userId == Guid.Empty)
                return false;

            var tokenHash = _refreshTokenHasher.Hash(refreshToken);
            var token = await _dbContext.RefreshTokens.SingleOrDefaultAsync(token =>
                token.TokenHash == tokenHash && token.UserId == userId && !token.IsRevoked);

            if (token == null)
                return false;

            Revoke(token, DateTime.UtcNow);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ValidateRefreshTokenAsync(string tokenString, string fingerprint)
        {
            if (string.IsNullOrWhiteSpace(tokenString))
                return false;

            var tokenHash = _refreshTokenHasher.Hash(tokenString);
            var token = await _dbContext.RefreshTokens.SingleOrDefaultAsync(token => token.TokenHash == tokenHash);

            return token is { IsActive: true } &&
                   string.Equals(token.Fingerprint, fingerprint, StringComparison.Ordinal);
        }

        private async Task RevokeFamilyAsync(Guid familyId, DateTime now)
        {
            var activeTokens = await _dbContext.RefreshTokens
                .Where(token => token.FamilyId == familyId && !token.IsRevoked)
                .ToListAsync();

            foreach (var token in activeTokens)
                Revoke(token, now);

            await _dbContext.SaveChangesAsync();
        }

        private static RefreshToken CreateToken(
            RefreshTokenCreationRequest request,
            string tokenHash,
            Guid familyId,
            DateTime now) => new()
            {
                TokenHash = tokenHash,
                FamilyId = familyId,
                CreatedAt = now,
                UpdatedAt = now,
                LastUsed = now,
                ExpiresAt = now.AddDays(30),
                AbsoluteExpiresAt = now.AddDays(90),
                DeviceName = request.DeviceName,
                Fingerprint = request.Fingerprint,
                UserId = request.UserId,
                UserAgent = request.UserAgent,
                CreatedByIp = request.CreatedByIp,
                IsRevoked = false,
                Type = GetBrowser(request.UserAgent)
            };

        private static void Revoke(RefreshToken token, DateTime now, string? replacedByTokenHash = null)
        {
            token.IsRevoked = true;
            token.RevokedAt = now;
            token.ReplacedByTokenHash = replacedByTokenHash;
            token.UpdatedAt = now;
            token.LastUsed = now;
        }

        private static DateTime Min(DateTime first, DateTime second) => first <= second ? first : second;

        private static UnauthorizedAccessException InvalidRefreshToken() =>
            new("Session expirée ou invalide. Veuillez vous reconnecter.");

        private static string GetBrowser(string? userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Inconnu";

            if (userAgent.Contains("Chrome", StringComparison.OrdinalIgnoreCase) && !userAgent.Contains("Edg") && !userAgent.Contains("OPR") && !userAgent.Contains("SamsungBrowser"))
                return "Chrome";
            if (userAgent.Contains("Firefox", StringComparison.OrdinalIgnoreCase))
                return "Firefox";
            if (userAgent.Contains("Edg", StringComparison.OrdinalIgnoreCase))
                return "Edge";
            if (userAgent.Contains("Safari", StringComparison.OrdinalIgnoreCase) && !userAgent.Contains("Chrome"))
                return "Safari";
            if (userAgent.Contains("OPR", StringComparison.OrdinalIgnoreCase) || userAgent.Contains("Opera", StringComparison.OrdinalIgnoreCase))
                return "Opera";
            if (userAgent.Contains("SamsungBrowser", StringComparison.OrdinalIgnoreCase))
                return "Samsung Internet";
            if (userAgent.Contains("ArzenalStoreManager", StringComparison.OrdinalIgnoreCase))
                return "ArzenalStoreManager";

            return "Inconnu";
        }

        private static string GenerateSecureRefreshToken() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}
