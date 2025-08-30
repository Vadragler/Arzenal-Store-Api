using ArzenalStoreApi.Models;
using ArzenalStoreSharedDto.DTOs.AuthDto;

namespace ArzenalStoreApi.Services.Token
{
    public interface IRefreshTokenService
    {
        
        Task<string> GenerateRefreshToken(CreateRefreshTokenDto dto);
        Task<RefreshToken?> RotateRefreshTokenAsync(string oldToken, string ipAddress, string fingerprint);
        Task<bool> RevokeRefreshTokenAsync(string token,string userIdStr);
        Task<bool> ValidateRefreshTokenAsync(string token, string fingerprint);
    }

}
