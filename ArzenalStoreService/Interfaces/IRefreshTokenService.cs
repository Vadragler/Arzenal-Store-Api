using Arzenal.Store.Api.Domain.Models.Requests;
using Arzenal.Store.Api.Service.Models.Auth;
using Microsoft.AspNetCore.Http;

namespace Arzenal.Store.Api.Service.Interfaces
{
    public interface IRefreshTokenService
    {
        
        Task<RefreshTokenCookieData> GenerateRefreshToken(RefreshTokenCreationRequest request);
        Task<RefreshTokenCookieData?> RotateRefreshTokenAsync(HttpContext context);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken,Guid? userId);
        Task<bool> ValidateRefreshTokenAsync(string tokenString, string fingerprint);
    }

}
