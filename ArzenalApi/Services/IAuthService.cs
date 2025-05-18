using Arzenal.Shared.Dtos.DTOs.RegisterDto;
using Microsoft.AspNetCore.Identity.Data;
using System.Threading.Tasks;

namespace ArzenalApi.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string? ErrorMessage)> RegisterAsync(RegisterRequestDto request);

        Task<string?> AuthenticateAsync(string username, string password);
        Task<(string? username, string email)> FindByIdAsync(string? userId);

        Task<bool> DeleteAsync(string userId);

        Task<bool> PatchAsync(string userId,string username,string email,string actualpassword,string newpassword);
    }
}
