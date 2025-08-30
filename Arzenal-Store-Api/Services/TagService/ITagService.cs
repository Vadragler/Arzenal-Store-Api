using ArzenalStoreSharedDto.DTOs.TagDto;

namespace ArzenalStoreApi.Services.TagService
{
    public interface ITagService
    {
        Task<IEnumerable<ReadTagDto>> GetAllAsync();
        Task<ReadTagDto> GetByIdAsync(int id);
        Task<ReadTagDto> CreateAsync(CreateTagDto dto);
        Task<ReadTagDto> UpdateAsync(int id, UpdateTagDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
