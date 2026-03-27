using Arzenal.Store.Api.Domain.Models;
using Arzenal.Dto.DTOs.LanguageDto;
using Arzenal.Dto.DTOs.LanguageDto;

namespace Arzenal.Store.Api.Service.Mapping.LanguageMapping
{
    public class LanguageMapperWrapper : ILanguageMapper
    {
        private readonly LanguageMapper _mapper;
        public LanguageMapperWrapper(LanguageMapper mapper) 
        {
            _mapper = mapper;
        }

        public ReadLanguageDto ToReadLanguageDto(Language language) => _mapper.ToReadLanguageDto(language);

        public Language ToLanguage(CreateLanguageDto dto) => _mapper.ToLanguage(dto);

        public void UpdateLanguageFromDto(UpdateLanguageDto dto, Language language)=> _mapper.UpdateLanguageFromDto(dto,language);
    }
}
