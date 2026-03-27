using Arzenal.Store.Api.Domain.Models;
using Arzenal.Dto.DTOs.LanguageDto;
using Arzenal.Dto.DTOs.LanguageDto;

namespace Arzenal.Store.Api.Service.Mapping.LanguageMapping
{
    public interface ILanguageMapper
    {
        public ReadLanguageDto ToReadLanguageDto(Language language);

        public Language ToLanguage(CreateLanguageDto dto);

        public void UpdateLanguageFromDto(UpdateLanguageDto dto, Language language);
    }
}
