using Arzenal.Store.Api.Domain.Models;
using Arzenal.Dto.DTOs.TagDto;
using Riok.Mapperly.Abstractions;

namespace Arzenal.Store.Api.Service.Mapping.TagMapping
{
    [Mapper]
    public partial class TagMapper
    {
        public partial ReadTagDto ToReadTagDto(Tag tag);
        public partial void UpdateTagFromDto(UpdateTagDto dto, Tag tag);

        // Méthode manuelle pour créer une entité sans Id
        public Tag ToTag(CreateTagDto dto)
        {
            return new Tag
            {
                Name = dto.Name // Id volontairement ignoré
            };
        }
    }

}
