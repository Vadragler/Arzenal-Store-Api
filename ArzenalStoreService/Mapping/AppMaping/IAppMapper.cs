using Arzenal.Store.Api.Domain.Models;
using Arzenal.Dto.DTOs.AppDto;

namespace Arzenal.Store.Api.Service.Mapping.AppMaping
{
    public interface IAppMapper
    {
        ReadAppDto ToReadAppDto(App app);
        App ToApp(CreateAppDto dto);
        void UpdateAppFromDto(UpdateAppDto dto, App app);
    }

}
