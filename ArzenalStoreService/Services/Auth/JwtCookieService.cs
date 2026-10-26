using Arzenal.Store.Api.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

namespace Arzenal.Store.Api.Service.Services.Auth
{
    public class JwtCookieService : IJwtCookieService
    {
        private static readonly string[] ValidAudiences = ["web", "ArzenalStoreManager", "ArzenalAuth"];
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICookieService _cookieService;
        private readonly IRefreshTokenHasher _refreshTokenHasher;

        public JwtCookieService(
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            ICookieService cookieService,
            IRefreshTokenHasher refreshTokenHasher)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _cookieService = cookieService;
            _refreshTokenHasher = refreshTokenHasher;
        }

        public Guid GetUserIdFromCookie()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return Guid.Empty;

            var jwt = context.Request.Cookies["authToken"];
            if (string.IsNullOrEmpty(jwt)) return Guid.Empty;

            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _configuration["Jwt:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new InvalidOperationException("La clé secrète JWT n'est pas configurée.");

            var key = Encoding.UTF8.GetBytes(secretKey);
                
            try
            {
                var principal = tokenHandler.ValidateToken(jwt, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    // Use the same audiences as configured for Jwt authentication
                    ValidAudiences = ValidAudiences,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return Guid.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            }
            catch(Exception ex)
            {
                // Propager l'exception d'origine dans le message pour faciliter le debug en tests
                throw new UnauthorizedAccessException(ex.Message);
            }
        }

        public bool GetCurrentCookie(string tokenHash)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return false;

            var currentRefreshToken = _cookieService.GetRefreshToken(context.Request);
            if (currentRefreshToken == null || string.IsNullOrWhiteSpace(currentRefreshToken.Token))
                return false;

            var currentTokenHash = _refreshTokenHasher.Hash(currentRefreshToken.Token);
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(currentTokenHash),
                Encoding.UTF8.GetBytes(tokenHash));
        }
    }
}
