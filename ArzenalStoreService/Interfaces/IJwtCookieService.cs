namespace Arzenal.Store.Api.Service.Interfaces
{
    public interface IJwtCookieService
    {
        Guid GetUserIdFromCookie();
        bool GetCurrentCookie(string token);
    }
}
