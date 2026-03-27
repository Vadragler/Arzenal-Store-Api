using Arzenal.Dto.DTOs.AccountDto;
using Arzenal.Dto.DTOs.AccountDto;

namespace Arzenal.Store.Api.Service.Interfaces
{
    public interface IUserService
    {
        Task<ReadAccountDto> FindByIdAsync(string? userId);

        Task DeleteAsync(string? userId);

        Task PatchAsync(string? userId,UpdateAccountDto updateAccountDto);
    }
}
