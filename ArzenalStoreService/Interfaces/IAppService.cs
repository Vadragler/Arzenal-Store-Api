using Arzenal.Dto.DTOs.AppDto;

namespace Arzenal.Store.Api.Service.Interfaces

{
    public interface IAppService
    {
        public Task<List<ReadAppDto>> GetAllAppsAsync();
        public Task<ReadAppDto> GetAppByIdAsync(Guid id);
        public Task<ReadAppDto> CreateAppAsync(CreateAppDto createAppDto);
        public Task<ReadAppDto> UpdateAppAsync(Guid id, UpdateAppDto Appdto);
        public Task<bool> DeleteAppAsync(Guid id);
    }
}
