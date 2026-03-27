using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.Service.Mapping.TagMapping;
using Arzenal.Dto.DTOs.TagDto;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Mapper
{
    public class TagMapperTests
    {
        private readonly TagMapper _mapper = new TagMapper();
        
      
        [Fact]
        public void ToReadTagDto_Maps_AllPrperties()
        {
            // Arrange
            var tag = new Tag()
            {
                Id = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"),
                Name = "TestTag"
            };

            // Act
            var dto = _mapper.ToReadTagDto(tag);

            // Assert
            Assert.Equal(tag.Id, dto.Id);
            Assert.Equal(tag.Name, dto.Name);

        }

        [Fact]
        public void ToTag_Maps_CreateDto_To_Tag()
        {
            // Arrange
            var tag = new CreateTagDto
            {
                Name = "TestTag"
            };

            // Act
            var dto = _mapper.ToTag(tag);

            // Assert
            Assert.Equal(dto.Name, tag.Name);
        }

        [Fact]
        public void UpdateTagFromDto_Maps_Update_Name()
        {
            // Arrange
            var dto = new UpdateTagDto
            {
                Name = "newTag"
            };

            var tag = new Tag()
            {
                Id = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"),
                Name = "OldTag"
            };

            // Act
            _mapper.UpdateTagFromDto(dto, tag);

            // Assert
            Assert.Equal(tag.Name, dto.Name);
        }

        [Fact]
        public void UpdateTagFromDto_DoesNotOverride_Id()
        {
            // Arrange
            var originalId = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d");
            var tag = new Tag { Id = originalId, Name = "OldTag" };
            var dto = new UpdateTagDto { Name = "NewOldTag" };

            // Act
            _mapper.UpdateTagFromDto(dto, tag);

            // Assert
            Assert.Equal(originalId, tag.Id);
        }

        [Fact]
        public void UpdateTagFromDto_WithNullValue_HandlesGracefully()
        {
            // Arrange
            var tag = new Tag { Id = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"), Name = "OldTag" };
            var dto = new UpdateTagDto { Name = null };

            // Act
            _mapper.UpdateTagFromDto(dto, tag);

            // Assert
            Assert.Null(tag.Name);
        }
    }
}
