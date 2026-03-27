using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Store.Api.Domain.Models.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Arzenal.Store.Api.Service.Services.Auth
{
    public class JwtCookieService : IJwtCookieService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtCookieService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid GetUserIdFromCookie()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return Guid.Empty;

            var jwt = context.Request.Cookies["authToken"];
            if (string.IsNullOrEmpty(jwt)) return Guid.Empty;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]);
                
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
                    ValidAudiences = new[] { "web", "ArzenalStoreManager", "ArzenalAuth" },
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

        public bool GetCurrentCookie(string token)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return false;

            var cookieValue = context.Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(cookieValue)) return false;

            try
            {
                var currentRefreshToken = JsonSerializer.Deserialize<RefreshTokenCookieData>(
                    Convert.FromBase64String(cookieValue)
                );

                if (currentRefreshToken == null) return false;

                return currentRefreshToken.Token == token;
            }
            catch
            {
                // Si le cookie est mal formé, on considère que ce n'est pas le bon
                return false;
            }
        }
    }
}
