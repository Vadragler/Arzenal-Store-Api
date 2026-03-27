using Arzenal.Dto.DTOs.OperatingSystemDto;

namespace Arzenal.Store.Api.Service.Interfaces
{
    public interface IOperatingSystemService
    {
        Task<List<ReadOperatingSystemDto>> GetAllAsync();
        Task<ReadOperatingSystemDto> GetByIdAsync(Guid id);
        Task<ReadOperatingSystemDto> CreateAsync(CreateOperatingSystemDto dto);
        Task<bool> UpdateAsync(Guid id, UpdateOperatingSystemDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
