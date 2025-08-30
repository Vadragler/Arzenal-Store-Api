using ArzenalStoreApi.Data;
using ArzenalStoreApi.Models;
using ArzenalStoreSharedDto.DTOs.AuthDto;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ArzenalStoreApi.Services.Token
{
    public class RefreshTokenService(AuthDbContext context) : IRefreshTokenService
    {
        private readonly AuthDbContext _dbContext = context;



        public async Task<string> GenerateRefreshToken(CreateRefreshTokenDto dto)
        {
            // Vérifie si l'utilisateur existe
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
            if (user == null)
                throw new InvalidOperationException("Utilisateur non trouvé.");

            var refreshToken = new RefreshToken
            {
                Token = GenerateSecureRefreshToken(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = null,
                DeviceName = dto.DeviceName,
                Fingerprint = dto.Fingerprint,
                UserId = dto.UserId,
                UserAgent = dto.UserAgent,
                CreatedByIp = dto.CreatedByIp,
                IsRevoked = false
            };

            _dbContext.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync();

            return refreshToken.Token;
        }

        private static string GenerateSecureRefreshToken(int size = 64)
        {
            var randomNumber = new byte[size];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<RefreshToken?> RotateRefreshTokenAsync(string oldToken, string ipAddress, string fingerprint)
        {
            if (string.IsNullOrEmpty(oldToken))
                throw new UnauthorizedAccessException("Refresh token manquant.");
            // Recherche le refresh token actif correspondant
            var token = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == oldToken);

            if (token == null)
                throw new UnauthorizedAccessException("Refresh token invalide ou expiré.");

            // Vérifie que le fingerprint correspond pour éviter le vol
            if (token.Fingerprint != fingerprint)
                throw new UnauthorizedAccessException("Fingerprint invalide.");

            // Génère un nouveau token sécurisé 
            var newTokenValue = GenerateSecureRefreshToken();

            // Mets à jour le token existant (patch)
            token.Token = newTokenValue;
            token.CreatedAt = DateTime.UtcNow;
            token.CreatedByIp = ipAddress;
            token.ExpiresAt = DateTime.UtcNow.AddMonths(6);
            token.RevokedAt = null;
            token.IsRevoked = false;
            token.ReplacedByToken = null; // Ou tu peux stocker l'ancien token si tu veux

            await _dbContext.SaveChangesAsync();

            return token;
        }


        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, string userIdStr)
        {
            if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(userIdStr) ||
       !Guid.TryParse(userIdStr, out var userId))
            {
                throw new UnauthorizedAccessException("Token ou identifiant utilisateur invalide.");
            }
            var token = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken && t.UserId == userId && !t.IsRevoked);

            if (token == null)
                throw new UnauthorizedAccessException("Refresh token invalide ou déjà révoqué.");

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
