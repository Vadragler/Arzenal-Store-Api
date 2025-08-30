using ArzenalStoreSharedDto.DTOs.AuthDto;

namespace ArzenalStoreApi.Services.RequestInfoProvider
{
    public interface IRequestInfoProvider
    {
        CreateRefreshTokenDto GetRequestInfo(HttpContext context, LoginRequestDto request);
    }

}
