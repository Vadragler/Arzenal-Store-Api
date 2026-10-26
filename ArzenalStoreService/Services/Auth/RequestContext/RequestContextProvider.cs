using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Dto.DTOs.ClientContextDto;
using Microsoft.AspNetCore.Http;

namespace Arzenal.Store.Api.Service.Services.Auth.RequestContext
{
    public sealed class RequestContextProvider : IRequestContextProvider
    {
        public ClientContextDto GetClientContext(HttpContext http)
        {
            var ua = http.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown User-Agent";
            var ip = http.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var fingerprint = http.Request.Headers["X-Fingerprint"].FirstOrDefault();
            var deviceName = http.Request.Headers["X-Device-Name"].FirstOrDefault();

            return new ClientContextDto
            {
                UserAgent = ua,
                IpAddress = ip,
                Fingerprint = fingerprint,
                DeviceName = deviceName
            };
        }
    }
}
