using Arzenal.Dto.DTOs.AuthDto;
using Microsoft.AspNetCore.Http;

namespace Arzenal.Store.Api.Service.Interfaces
{
    public interface IRequestInfoProvider
    {
         Task<CreateRefreshTokenDto> GetRequestInfo(HttpContext context, LoginRequestDto request);
    }

}
