using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.Service.Mapping.CategoryMapping;
using Arzenal.Dto.DTOs.CategorieDto;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Mapper
{
    public class CategoryMapperTests
    {
        private readonly CategoryMapper _mapper = new CategoryMapper();

        [Fact]
        public void ToReadCategoryDto_Maps_AllProperties()
        {
            // Arrange
            var category = new Categorie
            {
                Id = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"),
                Name = "TestCategory"
            };

            // Act
            var dto = _mapper.ToReadCategoryDto(category);

            // Assert
            Assert.Equal(category.Id, dto.Id);
            Assert.Equal(category.Name, dto.Name);
        }

        [Fact]
        public void ToCategory_Maps_CreateDto_To_Categorie()
        {
            // Arrange
            var dto = new CreateCategorieDto
            {
                Name = "NewCategory"
            };

            // Act
            var category = _mapper.ToCategory(dto);

            // Assert
            Assert.Equal(dto.Name, category.Name);
        }

        [Fact]
        public void UpdateCategoryFromDto_Updates_Name()
        {
            // Arrange
            var dto = new UpdateCategorieDto
            {
                Name = "UpdatedCategory"
            };
            var category = new Categorie
            {
                Id = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"),
                Name = "OldCategory"
            };

            // Act
            _mapper.UpdateCategoryFromDto(dto, category);

            // Assert
            Assert.Equal(dto.Name, category.Name);
        }

        [Fact]
        public void UpdateCategoryFromDto_DoesNotOverride_Id()
        {
            // Arrange
            var originalId = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d");
            var categorie = new Categorie { Id = originalId, Name = "OldCategory" };
            var dto = new UpdateCategorieDto { Name = "NewCategory" };

            // Act
            _mapper.UpdateCategoryFromDto(dto, categorie);

            // Assert
            Assert.Equal(originalId, categorie.Id);
        }

        [Fact]
        public void UpdateCategoryFromDto_WithNullValue_HandlesGracefully()
        {
            // Arrange
            var categorie = new Categorie { Id = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"), Name = "OldCategory"};
            var dto = new UpdateCategorieDto { Name = null };

            // Act
            _mapper.UpdateCategoryFromDto(dto, categorie);

            // Assert
            Assert.Equal("OldCategory", categorie.Name);
        }
    }
}
