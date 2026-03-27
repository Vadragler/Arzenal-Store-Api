using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Store.Api.Domain.Models.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace Arzenal.Store.Api.Service.Services.Auth.Cookies
{
    public sealed class CookieService(IConfiguration configuration) : ICookieService
    {
        private readonly IConfiguration _configuration = configuration;

        public RefreshTokenCookieData? GetRefreshToken(HttpRequest request)
        {
            var cookieValue = request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(cookieValue))
                return null;

            try
            {
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(cookieValue));
                return JsonSerializer.Deserialize<RefreshTokenCookieData>(
                Convert.FromBase64String(cookieValue));
            }
            catch
            {
                return null;
            }
        }

        public string? GetAuthToken(HttpRequest request)
        { 
            var jwt = request.Cookies["authToken"];
           
                if (string.IsNullOrEmpty(jwt))
                    throw new UnauthorizedAccessException("No auth token provided in cookies.");
                return jwt;
        }

        public void SetAuthCookies(HttpResponse response,string accessToken , RefreshTokenCookieData data)
        {
            var domain = _configuration["CookieDomain"];
            var cookieValue = Convert.ToBase64String(
               JsonSerializer.SerializeToUtf8Bytes(data));

            response.Cookies.Append("refreshToken", cookieValue, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Domain = domain,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                MaxAge = TimeSpan.FromDays(30)
            });

            response.Cookies.Append("authToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Domain = domain,
                Path = "/",
            });
        }

        public void DeleteAuthCookies(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;

            bool hadAuthToken = request.Cookies.ContainsKey("authToken");
            bool hadRefreshToken = request.Cookies.ContainsKey("refreshToken");
            var cookieoptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Domain = _configuration["CookieDomain"],
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(-1) // Expire in the past
            };
            response.Cookies.Delete("authToken", cookieoptions);
            response.Cookies.Delete("refreshToken", cookieoptions);

            if (!hadAuthToken && !hadRefreshToken)
                throw new InvalidOperationException("Aucun cookie d'authentification trouvé à supprimer.");
        }

        public void DeleteRefreshCookie(HttpResponse response)
        {
            response.Cookies.Delete("refreshToken");
        }
    }
}
