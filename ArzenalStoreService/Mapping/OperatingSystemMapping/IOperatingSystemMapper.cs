using Arzenal.Dto.DTOs.OperatingSystemDto;
using OperatingSystem = Arzenal.Store.Api.Domain.Models.OperatingSystem;

namespace Arzenal.Store.Api.Service.Mapping.OperatingSystemMapping
{
    public interface IOperatingSystemMapper
    {
        public ReadOperatingSystemDto ToReadOperatingSystemDto(OperatingSystem os);
        public OperatingSystem ToOperatingSystem(CreateOperatingSystemDto dto);
        public void UpdateOperatingSystemFromDto(UpdateOperatingSystemDto dto, OperatingSystem os);
    }
}
