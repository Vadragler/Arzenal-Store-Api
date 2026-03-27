using Arzenal.Store.Api.Domain.Models.Requests;
using System.Text.Json;

namespace Arzenal.Store.Api.TestIntegration.Infrastructure
{
    public class CookieInjectingHandler : DelegatingHandler
    {
        private readonly string _authToken;
        private readonly RefreshTokenCookieData _refreshData;
        private readonly string _fingerprint;

        public CookieInjectingHandler(string authToken, RefreshTokenCookieData refreshData, string fingerprint)
        {
            _authToken = authToken;
            _refreshData = refreshData;
            _fingerprint = fingerprint;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Remove("Cookie");
            request.Headers.TryAddWithoutValidation("Cookie", $"authToken={_authToken}");
            request.Headers.Remove("Fingerprint");
            request.Headers.Add("Fingerprint", _fingerprint);

            var encoded = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(_refreshData));
            request.Headers.TryAddWithoutValidation("Cookie", $"refreshToken={encoded}");

            return base.SendAsync(request, cancellationToken);
        }
    }

}
