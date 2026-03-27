using Arzenal.Dto.DTOs.CategorieDto;

namespace Arzenal.Store.Api.Service.Interfaces
{
    public interface ICategoryService
    {
        Task<List<ReadCategorieDto>> GetAllAsync();
        Task<ReadCategorieDto?> GetByIdAsync(Guid id);
        Task<ReadCategorieDto?> CreateAsync(CreateCategorieDto dto);
        Task<bool> UpdateAsync(Guid id, UpdateCategorieDto dto);
        Task<bool> DeleteAsync(Guid id);
    }

}
