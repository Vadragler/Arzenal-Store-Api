using Arzenal.Dto.DTOs.TagDto;

namespace Arzenal.Store.Api.Service.Interfaces
{
    public interface ITagService
    {
        Task<List<ReadTagDto>> GetAllAsync();
        Task<ReadTagDto> GetByIdAsync(Guid id);
        Task<ReadTagDto> CreateAsync(CreateTagDto dto);
        Task<bool> UpdateAsync(Guid id, UpdateTagDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
