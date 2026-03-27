using Microsoft.AspNetCore.Http;
using Arzenal.Dto.DTOs.ClientContextDto;

namespace Arzenal.Store.Api.Service.Interfaces
{
    public interface IRequestContextProvider
    {
        ClientContextDto Get(HttpContext http);
    }
}
