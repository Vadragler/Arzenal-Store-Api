using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.Service.Mapping.AppMaping;
using Arzenal.Dto.DTOs.AppDto;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Mapper
{
    public class AppMapperTests
    {
        private readonly AppMapper _mapper = new AppMapper();

        [Fact]
        public void ToReadAppDto_Maps_AllProperties()
        {
            // Arrange
            var app = new App
            {
                Id = Guid.NewGuid(),
                Name = "TestApp",
                Version = "1.0.0",
                Description = "A simple app"
            };

            // Act
            var dto = _mapper.ToReadAppDto(app);

            // Assert
            Assert.Equal(app.Id, dto.Id);
            Assert.Equal(app.Name, dto.Name);
            Assert.Equal(app.Version, dto.Version);
            Assert.Equal(app.Description, dto.Description);
        }

        [Fact]
        public void ToApp_Maps_CreateDto_To_App()
        {
            // Arrange
            var dto = new CreateAppDto
            {
                Name = "NewApp",
                Version = "2.3.4",
            };

            // Act
            var app = _mapper.ToApp(dto);

            // Assert
            Assert.Equal(dto.Name, app.Name);
            Assert.Equal(dto.Version, app.Version);
            
        }

        [Fact]
        public void UpdateAppFromDto_Updates_Name_And_Version()
        {
            // Arrange
            var dto = new UpdateAppDto
            {
                Name = "UpdatedApp",
                Version = "3.0.0"
            };
            var app = new App
            {
                Id = Guid.NewGuid(),
                Name = "OldName",
                Version = "1.0.0"
            };

            // Act
            _mapper.UpdateAppFromDto(dto, app);

            // Assert
            Assert.Equal(dto.Name, app.Name);
            Assert.Equal(dto.Version, app.Version);
            Assert.NotEqual(Guid.Empty, app.Id);
        }

        [Fact]
        public void UpdateAppFromDto_DoesNotOverride_Id()
        {
            // Arrange
            var originalId = Guid.NewGuid();
            var app = new App { Id = originalId, Name = "OldApp",Version = "1.0.0"};
            var dto = new UpdateAppDto { Name = "NewApp" };

            // Act
            _mapper.UpdateAppFromDto(dto, app);

            // Assert
            Assert.Equal(originalId, app.Id);
        }

        [Fact]
        public void UpdateAppFromDto_WithNullValue_HandlesGracefully()
        {
            // Arrange
            var app = new App { Id = Guid.NewGuid(), Name = "OldApp", Version = "1.0.0" };
            var dto = new UpdateAppDto { Name = null };

            // Act
            _mapper.UpdateAppFromDto(dto, app);

            // Assert
            Assert.Equal("OldApp", app.Name);
        }
    }
}
