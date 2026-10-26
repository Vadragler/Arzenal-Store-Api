using Arzenal.Store.Api.Domain.Models.Requests;
using System.Text.Json;

namespace Arzenal.Store.Api.TestIntegration.Infrastructure
{
    public class CookieInjectingHandler : DelegatingHandler
    {
        private readonly string _authToken;
        private readonly string _refreshCookie;
        private readonly string _bearerToken;
        private readonly string _fingerprint;

        public CookieInjectingHandler(string authToken, RefreshTokenCookieData refreshData, string fingerprint)
            : this(
                authToken,
                Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(refreshData)),
                authToken,
                fingerprint)
        {
        }

        public CookieInjectingHandler(string authToken, string refreshCookie, string bearerToken, string fingerprint)
        {
            _authToken = authToken;
            _refreshCookie = refreshCookie;
            _bearerToken = bearerToken;
            _fingerprint = fingerprint;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Remove("Cookie");
            request.Headers.TryAddWithoutValidation(
                "Cookie",
                $"authToken={_authToken}; refreshToken={_refreshCookie}");
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {_bearerToken}");
            request.Headers.Remove("Fingerprint");
            request.Headers.Add("Fingerprint", _fingerprint);

            return base.SendAsync(request, cancellationToken);
        }
    }

}
