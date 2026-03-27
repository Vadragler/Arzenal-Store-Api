using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.Domain.Models.Requests;
using Arzenal.Dto.DTOs.AuthDto;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Arzenal.Store.Api.Infrastructure.Data;


namespace Arzenal.Store.Api.Service.Services.Auth.Tokens
{
    public class RefreshTokenService(AuthDbContext context, ICookieService cookieService) : IRefreshTokenService
    {
        private readonly AuthDbContext _dbContext = context;
        private readonly ICookieService _cookieService = cookieService;

        public async Task<RefreshTokenCookieData> GenerateRefreshToken(CreateRefreshTokenDto dto)
        {
            // Vérifie si l'utilisateur existe
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
            if (user == null)
                throw new InvalidOperationException($"Utilisateur non trouvé. {dto.UserId}");

            string browser = await GetBrowser(dto.UserAgent);
            
            var refreshToken = new RefreshToken
            {
                Token = GenerateSecureRefreshToken(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                DeviceName = dto.DeviceName,
                Fingerprint = dto.Fingerprint,
                UserId = dto.UserId,
                UserAgent = dto.UserAgent,
                CreatedByIp = dto.CreatedByIp,
                IsRevoked = false,
                Type = browser,
            };

            var existingToken = await _dbContext.RefreshTokens
                .Where(t => t.UserId == dto.UserId &&
                            t.DeviceName == dto.DeviceName)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();
            if (existingToken == null)
                _dbContext.RefreshTokens.Add(refreshToken);
            else 
            {                 
                existingToken.Token = refreshToken.Token;
                existingToken.ExpiresAt = refreshToken.ExpiresAt;
                existingToken.IsRevoked = false;
            }
            await _dbContext.SaveChangesAsync();

            return new RefreshTokenCookieData
            {
                Token = refreshToken.Token,
                UserId = refreshToken.UserId
            };
        }

        private async Task<string> GetBrowser(string userAgent)
        {
            var browser = "Inconnu";
            if (!string.IsNullOrEmpty(userAgent))
            {
                var ua = userAgent;

                if (ua.Contains("Chrome", StringComparison.OrdinalIgnoreCase) && !ua.Contains("Edg") && !ua.Contains("OPR") && !ua.Contains("SamsungBrowser"))
                    browser = "Chrome";
                else if (ua.Contains("Firefox", StringComparison.OrdinalIgnoreCase))
                    browser = "Firefox";
                else if (ua.Contains("Edg", StringComparison.OrdinalIgnoreCase))
                    browser = "Edge";
                else if (ua.Contains("Safari", StringComparison.OrdinalIgnoreCase) && !ua.Contains("Chrome"))
                    browser = "Safari";
                else if (ua.Contains("OPR", StringComparison.OrdinalIgnoreCase) || ua.Contains("Opera", StringComparison.OrdinalIgnoreCase))
                    browser = "Opera";
                else if (ua.Contains("SamsungBrowser", StringComparison.OrdinalIgnoreCase))
                    browser = "Samsung Internet";
                else if (ua.Contains("CriOS", StringComparison.OrdinalIgnoreCase))
                    browser = "Chrome iOS";
                else if (ua.Contains("FxiOS", StringComparison.OrdinalIgnoreCase))
                    browser = "Firefox iOS";
                else if (ua.Contains("EdgiOS", StringComparison.OrdinalIgnoreCase))
                    browser = "Edge iOS";
                else if (ua.Contains("UCBrowser", StringComparison.OrdinalIgnoreCase))
                    browser = "UC Browser";
                else if (ua.Contains("YaBrowser", StringComparison.OrdinalIgnoreCase))
                    browser = "Yandex Browser";
                else if (ua.Contains("Brave", StringComparison.OrdinalIgnoreCase))
                    browser = "Brave";
                else if (ua.Contains("ArzenalStoreManager", StringComparison.OrdinalIgnoreCase))
                    browser = "ArzenalStoreManager";
                else
                    browser = "Inconnu";
            }
            return browser;
        }

        private static string GenerateSecureRefreshToken(int size = 64)
        {
            var randomNumber = new byte[size];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<RefreshTokenCookieData?> RotateRefreshTokenAsync(HttpContext context)
        {
            RefreshTokenCookieData? tokenValue = _cookieService.GetRefreshToken(context.Request);
            if (tokenValue == null)
                throw new UnauthorizedAccessException("Aucun token de rafraîchissement fourni.");
            RefreshToken? token = null;
            if (!string.IsNullOrEmpty(tokenValue.Token))
            {
                token = await _dbContext.RefreshTokens
                    .FirstOrDefaultAsync(t => t.Token == tokenValue.Token);
            }

            if (token == null)
            {
                string? fingerprint = context.Request.Headers["Fingerprint"];
                string? deviceName = context.Request.Headers["X-Device-Name"];
                Guid? userId = tokenValue.UserId;

                if (string.IsNullOrEmpty(fingerprint) || string.IsNullOrEmpty(deviceName) || userId == null)
                    throw new UnauthorizedAccessException("Données device incomplètes.");

                token = await _dbContext.RefreshTokens
                    .Where(t => t.UserId == userId &&
                                t.DeviceName == deviceName &&
                                !t.IsRevoked &&
                                t.ExpiresAt > DateTime.UtcNow)
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefaultAsync();

                if (token == null)
                    throw new UnauthorizedAccessException("Session expirée ou invalide. Veuillez vous reconnecter.");
            }
            // Vérifie que le fingerprint correspond pour éviter le vol
            if (token.Fingerprint != context.Request.Headers["Fingerprint"])
                throw new UnauthorizedAccessException("Fingerprint invalide.");

            // Génère un nouveau token sécurisé 
            var newTokenValue = GenerateSecureRefreshToken();

            // Mets à jour le token existant (patch)
            token.Token = newTokenValue;
            token.CreatedByIp = context.Connection.RemoteIpAddress?.ToString();
            token.CreatedAt = DateTime.UtcNow;
            token.ExpiresAt = DateTime.UtcNow.AddDays(30);
            token.LastUsed = DateTime.UtcNow;
            token.IsRevoked = false;
            token.ReplacedByToken = null;

            await _dbContext.SaveChangesAsync();

            return new RefreshTokenCookieData
            {
                Token = token.Token,
                UserId = token.UserId
            };
        }


        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, Guid? userId)
        {
            if (string.IsNullOrEmpty(refreshToken) || userId == null || userId == Guid.Empty)
            {
                return false;
            }
            var token = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken && t.UserId == userId && !t.IsRevoked);

            if (token == null)
                return false;

            _dbContext.RefreshTokens.Remove(token);

            await _dbContext.SaveChangesAsync();
            return true;
        }


        public async Task<bool> ValidateRefreshTokenAsync(string tokenString, string fingerprint)
        {
            var token = await _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == tokenString);

            if (token == null || !token.IsActive)
                return false;

            if (fingerprint != null && token.Fingerprint != fingerprint)
                return false;

            return true;
        }
    }
}
