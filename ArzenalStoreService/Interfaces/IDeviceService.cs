using Arzenal.Dto.DTOs.AccountDto;
using Microsoft.AspNetCore.Http;

namespace Arzenal.Store.Api.Service.Interfaces
{
    public interface IDeviceService
    {
        Task<List<ReadDeviceDto>> GetAllDevice(HttpRequest Request);
        Task<bool> RevokeDevice(Guid deviceId);
    }
}
