namespace ArzenalStoreApi.Services.RequestContextProvider
{
    public sealed class RequestContextProvider : IRequestContextProvider
    {
        public ClientContext Get(HttpContext http)
        {
            var ua = http.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown User-Agent";
            var ip = http.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var fingerprint = http.Request.Headers["X-Fingerprint"].FirstOrDefault();

            return new ClientContext
            {
                UserAgent = ua,
                IpAddress = ip,
                Fingerprint = fingerprint
            };
        }
    }

    public sealed class ClientContext
    {
        public string UserAgent { get; init; } = "";
        public string IpAddress { get; init; } = "unknown";
        public string? Fingerprint { get; init; }
    }
}
