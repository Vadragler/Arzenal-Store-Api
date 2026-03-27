using Arzenal.Store.Api.Domain.Models;
using Arzenal.Dto.DTOs.CategorieDto;

namespace Arzenal.Store.Api.Service.Mapping.CategoryMapping
{
    public interface ICategoryMapper
    {
        ReadCategorieDto ToReadCategoryDto(Categorie category);
        Categorie ToCategory(CreateCategorieDto dto);
        void UpdateCategoryFromDto(UpdateCategorieDto dto, Categorie category);
    }
}
