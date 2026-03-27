using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.Service.Mapping.LanguageMapping;
using Arzenal.Dto.DTOs.LanguageDto;


namespace Arzenal.Store.Api.TestUnitaire.Unit.Mapper
{
    public class LanguageMapperTests
    {
        private readonly LanguageMapper _mapper = new LanguageMapper();

        public void ToReadLanguageDto_Maps_AllPrperties() 
        {
            // Arrange
            var language = new Language
            {
                Id = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"),
                Name = "TestLanguage",
            };

            // Act
            var dto = _mapper.ToReadLanguageDto(language);

            // Assert
            Assert.Equal(dto.Id, language.Id);
            Assert.Equal(dto.Name, language.Name);
        }

        public void ToLanguage_Maps_CreateDto_To_Language()
        {
            // Arrange
            var language = new CreateLanguageDto
            {
                Name = "TestLanguage"
            };

            // Act
            var dto = _mapper.ToLanguage(language);

            // Assert
            Assert.Equal(dto.Name,language.Name);
        }

        public void UpdateLanguageFromDto_Maps_Update_Name()
        {
            // Arrange
            var dto = new UpdateLanguageDto
            {
                Name = "newLanguage"
            };
            var language = new Language
            {
                Id = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"),
                Name = "oldLanguge"
            };

            // Act
            _mapper.UpdateLanguageFromDto(dto, language);

            // Assert
            Assert.Equal(dto.Name, language.Name);
        }

        [Fact]
        public void UpdateLanguageFromDto_DoesNotOverride_Id()
        {
            // Arrange
            var originalId = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d");
            var language = new Language { Id = originalId, Name = "OldLanguage" };
            var dto = new UpdateLanguageDto { Name = "NewLanguage" };

            // Act
            _mapper.UpdateLanguageFromDto(dto, language);

            // Assert
            Assert.Equal(originalId, language.Id);
        }

        [Fact]
        public void UpdateLanguageFromDto_WithNullValue_HandlesGracefully()
        {
            // Arrange
            var language = new Language { Id = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"), Name = "OldLanguage" };
            var dto = new UpdateLanguageDto { Name = null };

            // Act
            _mapper.UpdateLanguageFromDto(dto, language);

            // Assert
            Assert.Null(language.Name);
        }
    }
}
