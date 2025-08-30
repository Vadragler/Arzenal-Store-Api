using ArzenalStoreSharedDto.DTOs.AuthDto;

namespace ArzenalStoreApi.Services.RequestInfoProvider
{
    public class RequestInfoProvider : IRequestInfoProvider
    {
        public CreateRefreshTokenDto GetRequestInfo(HttpContext context, LoginRequestDto request)
        {
            var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault();
            var createdByIp = context.Connection.RemoteIpAddress?.ToString();

            return new CreateRefreshTokenDto
            {
                DeviceName = request.DeviceName,
                Fingerprint = request.Fingerprint,
                UserAgent = userAgent ?? "Unknown User-Agent",
                CreatedByIp = createdByIp
            };
        }
    }

    
}
