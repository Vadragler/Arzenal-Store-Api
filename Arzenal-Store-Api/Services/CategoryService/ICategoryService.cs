using ArzenalStoreSharedDto.DTOs.CategorieDto;

namespace ArzenalStoreApi.Services.CategoryService
{
    public interface ICategoryService
    {
        Task<IEnumerable<ReadCategorieDto>> GetAllAsync();
        Task<ReadCategorieDto?> GetByIdAsync(int id);
        Task<ReadCategorieDto?> CreateAsync(CreateCategorieDto dto);
        Task<bool> UpdateAsync(int id, UpdateCategorieDto dto);
        Task<bool> DeleteAsync(int id);
    }

}
