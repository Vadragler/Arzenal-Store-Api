using Arzenal.Store.Api.Domain.Models.Requests;
using Arzenal.Dto.DTOs.AuthDto;
using Microsoft.AspNetCore.Http;

namespace Arzenal.Store.Api.Service.Interfaces
{
    public interface IRefreshTokenService
    {
        
        Task<RefreshTokenCookieData> GenerateRefreshToken(CreateRefreshTokenDto dto);
        Task<RefreshTokenCookieData?> RotateRefreshTokenAsync(HttpContext context);
        Task<bool> RevokeRefreshTokenAsync(string token,Guid? userId);
        Task<bool> ValidateRefreshTokenAsync(string token, string fingerprint);
    }

}
