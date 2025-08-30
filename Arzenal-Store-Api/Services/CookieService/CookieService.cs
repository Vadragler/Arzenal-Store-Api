using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArzenalStoreApi.Services.CookieService
{
    public sealed class CookieService : ICookieService
    {
        public string? GetRefreshToken(HttpRequest request)
            => request.Cookies["refreshToken"];

        public string? GetAuthToken(HttpRequest request)
        { 
            var jwt = request.Cookies["authToken"];
           
                if (string.IsNullOrEmpty(jwt))
                    throw new UnauthorizedAccessException("No auth token provided in cookies.");
                return jwt;
        }
        public void SetAuthCookies(HttpResponse response, string accessToken, string refreshToken)
        {
            response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // toujours true en prod
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            response.Cookies.Append("authToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(1)
            });
        }

        public void DeleteAuthCookies(HttpResponse response)
        {
            response.Cookies.Delete("refreshToken");
            response.Cookies.Delete("authToken");
        }

        public void DeleterefreshCookie(HttpResponse response)
        {
            response.Cookies.Delete("refreshToken");
        }
    }

}
