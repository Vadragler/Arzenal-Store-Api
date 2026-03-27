using Arzenal.Store.Api.Domain.Models;
using Arzenal.Dto.DTOs.CategorieDto;
using Riok.Mapperly.Abstractions;

namespace Arzenal.Store.Api.Service.Mapping.CategoryMapping
{
    [Mapper]
    public partial class CategoryMapper
    {
        public partial ReadCategorieDto ToReadCategoryDto(Categorie category);

        public partial Categorie ToCategory(CreateCategorieDto dto);

        public partial void UpdateCategoryFromDto(UpdateCategorieDto dto, Categorie category);
    }
}
