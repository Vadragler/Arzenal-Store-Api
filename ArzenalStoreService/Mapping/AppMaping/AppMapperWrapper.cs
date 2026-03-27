using Arzenal.Store.Api.Domain.Models;
using Arzenal.Dto.DTOs.AppDto;

namespace Arzenal.Store.Api.Service.Mapping.AppMaping
{
    public class AppMapperWrapper : IAppMapper
    {
        private readonly AppMapper _mapper;

        public AppMapperWrapper(AppMapper mapper)
        {
            _mapper = mapper;
        }

        public ReadAppDto ToReadAppDto(App app) => _mapper.ToReadAppDto(app);
        public App ToApp(CreateAppDto dto) => _mapper.ToApp(dto);
        public void UpdateAppFromDto(UpdateAppDto dto, App app) => _mapper.UpdateAppFromDto(dto, app);
    }

}
