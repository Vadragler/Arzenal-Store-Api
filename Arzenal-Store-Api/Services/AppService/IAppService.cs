using ArzenalStoreSharedDto.DTOs.AppDto;

namespace ArzenalStoreApi.Services.AppService

{
    public interface IAppService
    {
        public Task<List<ReadAppDto>> GetAllAppsAsync();
        public Task<ReadAppDto> GetAppByIdAsync(Guid id);
        public Task<ReadAppDto> CreateAppAsync(CreateAppDto createAppDto);
        public Task<UpdateAppDto> UpdateAppAsync(Guid id, UpdateAppDto Appdto);
        public Task<bool> DeleteAppAsync(Guid id);
    }
}
