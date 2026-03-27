using Arzenal.Dto.DTOs.OperatingSystemDto;
using Riok.Mapperly.Abstractions;
using OperatingSystem = Arzenal.Store.Api.Domain.Models.OperatingSystem;

namespace Arzenal.Store.Api.Service.Mapping.OperatingSystemMapping
{
    [Mapper]
    public partial class OperatingSystemMapper
    {
        public partial ReadOperatingSystemDto ToReadOperatingSystemDto(OperatingSystem os);
        public partial OperatingSystem ToOperatingSystem(CreateOperatingSystemDto dto);
        public partial void UpdateOperatingSystemFromDto(UpdateOperatingSystemDto dto,OperatingSystem  os);
    }
}
