namespace Arzenal.Store.Api.Domain.Models.Requests
{
    public class RefreshTokenCookieData
    {
        public string Token { get; set; } = string.Empty;
        public Guid UserId { get; set; }
    }
}
