namespace ArzenalStoreApi.Services.CookieService
{
    public interface ICookieService
    {
        string? GetRefreshToken(HttpRequest request);
        string? GetAuthToken(HttpRequest request);
        void SetAuthCookies(HttpResponse response, string accessToken, string refreshToken);
        void DeleteAuthCookies(HttpResponse response);

        void DeleterefreshCookie(HttpResponse response);
    }

}
