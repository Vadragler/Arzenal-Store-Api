using Arzenal.Store.Api.Domain.Models;
using Arzenal.Dto.DTOs.CategorieDto;

namespace Arzenal.Store.Api.Service.Mapping.CategoryMapping
{
    public class CategoryMapperWrapper : ICategoryMapper
    {
        private readonly CategoryMapper _mapper;
        public CategoryMapperWrapper(CategoryMapper mapper)
        {
            _mapper = mapper;
        }
        public ReadCategorieDto ToReadCategoryDto(Categorie category) => _mapper.ToReadCategoryDto(category);
        public Categorie ToCategory(CreateCategorieDto dto) => _mapper.ToCategory(dto);
        public void UpdateCategoryFromDto(UpdateCategorieDto dto, Categorie category) => _mapper.UpdateCategoryFromDto(dto, category);
    }
}
