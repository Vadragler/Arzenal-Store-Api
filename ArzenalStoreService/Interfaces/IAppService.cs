using Arzenal.Dto.DTOs.AppDto;

using Arzenal.Dto.DTOs.Pagination;

namespace Arzenal.Store.Api.Service.Interfaces

{
    public interface IAppService
    {
        public Task<List<ReadAppDto>> GetAllAppsAsync();
        public Task<PagedResultDto<ReadAppDto>> GetAppsPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        public Task<ReadAppDto> GetAppByIdAsync(Guid id);
        public Task<ReadAppDto> CreateAppAsync(CreateAppDto Appdto);
        public Task<ReadAppDto> UpdateAppAsync(Guid id, UpdateAppDto dto);
        public Task<bool> DeleteAppAsync(Guid id);
    }
}
