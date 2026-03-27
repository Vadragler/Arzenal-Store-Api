using Arzenal.Store.Api.Infrastructure.Data;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Arzenal.Store.Api.Service.Services.Auth.Tokens
{
    public class JwtService(AuthDbContext context, IConfiguration configuration, ICookieService cookieService) : IJwtService
    {
        private readonly AuthDbContext _dbContext = context;
        private readonly IConfiguration _configuration = configuration;
        private readonly ICookieService _cookieService = cookieService;
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
                Expires = DateTime.UtcNow.AddHours(double.Parse(_configuration["Jwt:ExpiryHours"] ?? "1")),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = null, // facultatif, on gère via les claims
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        public async Task<string> GenerateJwtTokenForAppAsync(HttpRequest request)
        {
            var appIdHeader = request.Headers["X-App-Id"].FirstOrDefault();
            if (string.IsNullOrEmpty(appIdHeader))
                throw new UnauthorizedAccessException("Le header X-App-Id est requis.");

            var refreshtoken = _cookieService.GetRefreshToken(request);
            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Id == refreshtoken.UserId);
            if (user == null)
                throw new UnauthorizedAccessException($"Utilisateur non trouvé {user}");

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
                    new Claim(ClaimTypes.Email, user.Email),
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
