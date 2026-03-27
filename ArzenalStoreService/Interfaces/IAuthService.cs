using Arzenal.Dto.DTOs.AuthDto;
using Microsoft.AspNetCore.Http;

namespace Arzenal.Store.Api.Service.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterRequestDto request);

        Task<(string? accessToken, string? refreshToken)> AuthenticateAsync(HttpResponse Response, string username, string password, CreateRefreshTokenDto dto);

        Task RotateRefreshTokenAsync(HttpContext context);

        Task GenerateRefreshTokenAsync(HttpContext context, CreateRefreshTokenDto dto);

        Task RevokeRefreshTokenAsync(HttpContext context);

        Task LogoutAsync(HttpContext context);
    }
}
