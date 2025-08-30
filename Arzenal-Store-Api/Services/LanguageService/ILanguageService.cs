using ArzenalStoreSharedDto.DTOs.LanguageDto;

namespace ArzenalStoreApi.Services.LanguageService
{
    public interface ILanguageService
    {
        Task<IEnumerable<ReadLanguageDto>> GetAllAsync();
        Task<ReadLanguageDto?> GetByIdAsync(int id);
        Task<ReadLanguageDto?> CreateAsync(CreateLanguageDto dto);
        Task<bool> UpdateAsync(int id, UpdateLanguageDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
