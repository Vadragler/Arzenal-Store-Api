using ArzenalStoreApi.Models;
using ArzenalStoreSharedDto.DTOs.AuthDto;

namespace ArzenalStoreApi.Services.Auth
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterRequestDto request);

        Task<(string? accessToken, string? refreshToken)> AuthenticateAsync(string username, string password, CreateRefreshTokenDto dto);

        Task LogoutAsync(string? refreshToken, string userIdStr);
    }
}
