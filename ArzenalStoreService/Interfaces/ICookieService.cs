using Arzenal.Store.Api.Domain.Models.Requests;
using Microsoft.AspNetCore.Http;

namespace Arzenal.Store.Api.Service.Interfaces
{
    public interface ICookieService
    {
        RefreshTokenCookieData? GetRefreshToken(HttpRequest request);
        string? GetAuthToken(HttpRequest request);
        void SetAuthCookies(HttpResponse response, string accessToken, RefreshTokenCookieData data);
        void DeleteAuthCookies(HttpContext context);
        void DeleteRefreshCookie(HttpResponse response);
    }
}
