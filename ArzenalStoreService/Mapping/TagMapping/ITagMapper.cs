using Arzenal.Store.Api.Domain.Models;
using Arzenal.Dto.DTOs.TagDto;

namespace Arzenal.Store.Api.Service.Mapping.TagMapping
{
    public interface ITagMapper
    {
        public ReadTagDto ToReadTagDto(Tag tag);
        public Tag ToTag(CreateTagDto dto);
        public void UpdateTagFromDto(UpdateTagDto dto, Tag tag);
    }
}
