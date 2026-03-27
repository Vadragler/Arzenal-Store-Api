using Arzenal.Store.Api.Service.Mapping.OperatingSystemMapping;
using Arzenal.Dto.DTOs.OperatingSystemDto;
using OperatingSystem = Arzenal.Store.Api.Domain.Models.OperatingSystem;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Mapper
{
    public class OperatingSystemTests
    {
        private readonly OperatingSystemMapper _mapper = new OperatingSystemMapper();

        [Fact]
        public void ToReadOperatingSystem_Maps_AllProperties()
        {
            // Arrange
            var operatingSystem = new OperatingSystem
            {
                Id = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"),
                Name = "OperatingSystemTest"
            };

            // Act
            var dto = _mapper.ToReadOperatingSystemDto(operatingSystem);

            // Assert
            Assert.Equal(dto.Id, operatingSystem.Id);
            Assert.Equal(dto.Name, operatingSystem.Name);
        }

        [Fact]
        public void CreateOperatingSystemFromDto()
        {
            // Arrange
            var operatingSystem = new CreateOperatingSystemDto
            {
                Name = "OperatingSystemTest"
            };

            // Act
            var dto = _mapper.ToOperatingSystem(operatingSystem);

            // Assert
            Assert.Equal(dto.Name, operatingSystem.Name);
        }

        [Fact]
        public void UpdateOperatingSystemFromDto()
        {
            // Arrange
            var dto = new UpdateOperatingSystemDto
            {
                Name = "NewOperatingSystem"
            };

            var operatingSystem = new OperatingSystem
            {
                Id = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"),
                Name = "OldOperatingSystem"
            };

            // Act
            _mapper.UpdateOperatingSystemFromDto(dto, operatingSystem);

            // Assert
            Assert.Equal(dto.Name, operatingSystem.Name);
        }

        [Fact]
        public void UpdateOperatingSystemFromDto_DoesNotOverride_Id()
        {
            // Arrange
            var originalId = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d");
            var operatingSystem = new OperatingSystem { Id = originalId, Name = "OldOperatingSystem" };
            var dto = new UpdateOperatingSystemDto { Name = "NewOldOperatingSystem" };

            // Act
            _mapper.UpdateOperatingSystemFromDto(dto, operatingSystem);

            // Assert
            Assert.Equal(originalId, operatingSystem.Id);
        }

        [Fact]
        public void UpdateOperatingSystemFromDto_WithNullValue_HandlesGracefully()
        {
            // Arrange
            var operatingSystem = new OperatingSystem { Id = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"), Name = "OldOperatingSystem" };
            var dto = new UpdateOperatingSystemDto { Name = null };

            // Act
            _mapper.UpdateOperatingSystemFromDto(dto, operatingSystem);

            // Assert
            Assert.Null(operatingSystem.Name);
        }
    }
}
