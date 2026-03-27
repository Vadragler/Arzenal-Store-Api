using Arzenal.Dto.DTOs.LanguageDto;
using Arzenal.Dto.DTOs.LanguageDto;

namespace Arzenal.Store.Api.Service.Interfaces
{
    public interface ILanguageService
    {
        Task<List<ReadLanguageDto>> GetAllAsync();
        Task<ReadLanguageDto> GetByIdAsync(Guid id);
        Task<ReadLanguageDto> CreateAsync(CreateLanguageDto dto);
        Task<bool> UpdateAsync(Guid id, UpdateLanguageDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
