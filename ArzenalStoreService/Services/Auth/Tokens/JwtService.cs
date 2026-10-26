using Arzenal.Store.Api.Infrastructure.Data;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Globalization;
using System.Security.Claims;
using System.Text;

namespace Arzenal.Store.Api.Service.Services.Auth.Tokens
{
    public class JwtService(
        AuthDbContext context,
        IConfiguration configuration,
        ICookieService cookieService,
        IRefreshTokenHasher refreshTokenHasher) : IJwtService
    {
        private readonly AuthDbContext _dbContext = context;
        private readonly IConfiguration _configuration = configuration;
        private readonly ICookieService _cookieService = cookieService;
        private readonly IRefreshTokenHasher _refreshTokenHasher = refreshTokenHasher;
        public async Task<string> GenerateJwtTokenAsync(Guid userId, List<string> audiences)
        {
            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new NotFoundException("Utilisateur non trouvé");

            var secretKey = _configuration["Jwt:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
                throw new InvalidOperationException("La clé secrète JWT n'est pas configurée.");

            var key = Encoding.UTF8.GetBytes(secretKey);
            var tokenHandler = new JwtSecurityTokenHandler();

            // Crée les claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            // Ajoute un claim par audience
            foreach (var aud in audiences)
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Aud, aud));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(double.Parse(_configuration["Jwt:ExpiryHours"] ?? "1", CultureInfo.InvariantCulture)),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = null, // facultatif, on gère via les claims
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        public async Task<string> GenerateJwtTokenForAppAsync(Guid userId, HttpRequest request)
        {
            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("Identité utilisateur invalide.");

            var appIdHeader = request.Headers["X-App-Id"].FirstOrDefault()?.Trim();
            if (string.IsNullOrEmpty(appIdHeader))
                throw new UnauthorizedAccessException("Le header X-App-Id est requis.");

            var allowedAppIds = _configuration.GetSection("AppLogin:AllowedAppIds")
                .GetChildren()
                .Select(section => section.Value)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Cast<string>()
                .ToArray();
            if (!allowedAppIds.Contains(appIdHeader, StringComparer.Ordinal))
                throw new UnauthorizedAccessException("Application non autorisée.");

            var refreshToken = _cookieService.GetRefreshToken(request);
            if (refreshToken == null || string.IsNullOrWhiteSpace(refreshToken.Token))
                throw new UnauthorizedAccessException("Un refresh token valide est requis.");

            var now = DateTime.UtcNow;
            var refreshTokenHash = _refreshTokenHasher.Hash(refreshToken.Token);
            var hasActiveRefreshToken = await _dbContext.RefreshTokens.AnyAsync(token =>
                token.UserId == userId &&
                token.TokenHash == refreshTokenHash &&
                !token.IsRevoked &&
                (token.ExpiresAt == null || token.ExpiresAt > now));

            if (!hasActiveRefreshToken)
                throw new UnauthorizedAccessException("Un refresh token valide est requis.");

            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new UnauthorizedAccessException("Utilisateur non trouvé.");

            var email = user.Email;
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidOperationException("L'utilisateur ne possède pas d'adresse e-mail valide.");

            var secretKey = _configuration["Jwt-App-Login:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
                throw new InvalidOperationException("La clé secrète Jwt-App-Login n'est pas configurée.");

            var key = Encoding.UTF8.GetBytes(secretKey);
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                    new Claim(JwtRegisteredClaimNames.Aud, "app-login"),
                    new Claim("appId", appIdHeader)
                }),
                Expires = DateTime.UtcNow.AddMinutes(10),
                Issuer = _configuration["Jwt-App-Login:Issuer"],
                Audience = "app-login",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
