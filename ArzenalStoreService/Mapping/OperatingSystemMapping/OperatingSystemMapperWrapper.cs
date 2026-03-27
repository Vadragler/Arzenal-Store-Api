using Arzenal.Dto.DTOs.OperatingSystemDto;
using OperatingSystem = Arzenal.Store.Api.Domain.Models.OperatingSystem;

namespace Arzenal.Store.Api.Service.Mapping.OperatingSystemMapping
{
    public class OperatingSystemMapperWrapper : IOperatingSystemMapper
    {
        private readonly OperatingSystemMapper _mapper;

        public OperatingSystemMapperWrapper(OperatingSystemMapper mapper)
        {
            _mapper = mapper;
        }
        public ReadOperatingSystemDto ToReadOperatingSystemDto(OperatingSystem os) => _mapper.ToReadOperatingSystemDto(os);
        public OperatingSystem ToOperatingSystem(CreateOperatingSystemDto dto) => _mapper.ToOperatingSystem(dto);
        public void UpdateOperatingSystemFromDto(UpdateOperatingSystemDto dto, OperatingSystem os) => _mapper.UpdateOperatingSystemFromDto(dto, os);

    }
}
