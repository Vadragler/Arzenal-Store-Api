using ArzenalStoreSharedDto.DTOs.AccountDto;

namespace ArzenalStoreApi.Services.UserService
{
    public interface IUserService
    {
        Task<ReadAccountDto> FindByIdAsync(string? userId);

        Task DeleteAsync(string userId);

        Task PatchAsync(string userId,UpdateAccountDto updateAccountDto);
    }
}
