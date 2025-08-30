using ArzenalStoreSharedDto.DTOs.OperatingSystemDto;

namespace ArzenalStoreApi.Services.OperatingSystemService
{
    public interface IOperatingSystemService
    {
        Task<IEnumerable<ReadOperatingSystemDto>> GetAllAsync();
        Task<ReadOperatingSystemDto> GetByIdAsync(int id);
        Task<ReadOperatingSystemDto> CreateAsync(CreateOperatingSystemDto dto);
        Task<ReadOperatingSystemDto> UpdateAsync(int id, UpdateOperatingSystemDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
