namespace Arzenal.Store.Api.Domain.Models.Requests
{
    public class RefreshTokenCookieData
    {
        public string Token { get; set; } = string.Empty;

        // Valeur réservée aux échanges internes serveur. Elle n'est jamais sérialisée dans le cookie.
        public Guid UserId { get; set; }
    }
}
