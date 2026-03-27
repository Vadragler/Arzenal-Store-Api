using Arzenal.Store.Api.Domain.Models;
using Arzenal.Dto.DTOs.TagDto;

namespace Arzenal.Store.Api.Service.Mapping.TagMapping
{
    public class TagMapperWrapper : ITagMapper
    {
        private readonly TagMapper _mapper;

        public TagMapperWrapper(TagMapper mapper)
        {
            _mapper = mapper;
        }

        public ReadTagDto ToReadTagDto(Tag tag) => _mapper.ToReadTagDto(tag);
        public Tag ToTag(CreateTagDto dto) => _mapper.ToTag(dto);
        public void UpdateTagFromDto(UpdateTagDto dto,Tag tag) => _mapper.UpdateTagFromDto(dto,tag);

    }
}
