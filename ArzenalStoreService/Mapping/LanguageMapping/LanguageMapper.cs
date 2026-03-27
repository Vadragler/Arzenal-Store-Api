using Arzenal.Store.Api.Domain.Models;
using Arzenal.Dto.DTOs.LanguageDto;
using Arzenal.Dto.DTOs.LanguageDto;
using Riok.Mapperly.Abstractions;

namespace Arzenal.Store.Api.Service.Mapping.LanguageMapping
{
    [Mapper]
    public partial class LanguageMapper
    {
        public partial ReadLanguageDto ToReadLanguageDto(Language language);

        public partial Language ToLanguage(CreateLanguageDto  dto);

        public partial void UpdateLanguageFromDto(UpdateLanguageDto dto, Language language);
    }
}
