using Arzenal.Dto.DTOs.OperatingSystemDto;
using Arzenal.Store.Api.Service.Interfaces;
using ArzenalStoreApi.Controllers.Apps;
using Moq;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Controllers.OperatingSystemsControllerTests
{
    public class OperatingSystemsErrorTests
    {
        private readonly Mock<IOperatingSystemService> _osServiceMock;
        private readonly OperatingSystemsController _controller;

        public OperatingSystemsErrorTests()
        {
            _osServiceMock = new Mock<IOperatingSystemService>();
            _controller = new OperatingSystemsController(_osServiceMock.Object);
        }

        [Fact]
        public async Task GetAllOperatingSystems_ThrowsException()
        {
            // Arrange
            _osServiceMock.Setup(s => s.GetAllAsync())
                .ThrowsAsync(new Exception("OS introuvable"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.GetAllOperatingSystems());
            Assert.Equal("OS introuvable", ex.Message);
        }

        [Fact]
        public async Task GetOperatingSystemById_ThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _osServiceMock.Setup(s => s.GetByIdAsync(id))
                .ThrowsAsync(new Exception("OS introuvable"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.GetOperatingSystemById(id));
            Assert.Equal("OS introuvable", ex.Message);
        }

        [Fact]
        public async Task PostOperatingSystem_ThrowsException()
        {
            // Arrange
            var createDto = new CreateOperatingSystemDto { Name = "Duplicate" };
            _osServiceMock.Setup(s => s.CreateAsync(createDto))
                .ThrowsAsync(new Exception("Cette OS existe déjà"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.PostOperatingSystem(createDto));
            Assert.Equal("Cette OS existe déjà", ex.Message);
        }

        [Fact]
        public async Task PutOperatingSystem_ThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var updateDto = new UpdateOperatingSystemDto { Name = "UpdateError" };
            _osServiceMock.Setup(s => s.UpdateAsync(id, updateDto))
                .ThrowsAsync(new Exception("Erreur update"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.PutOperatingSystem(id, updateDto));
            Assert.Equal("Erreur update", ex.Message);
        }

        [Fact]
        public async Task DeleteOperatingSystem_ThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _osServiceMock.Setup(s => s.DeleteAsync(id))
                .ThrowsAsync(new Exception("Erreur suppression"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.DeleteOperatingSystem(id));
            Assert.Equal("Erreur suppression", ex.Message);
        }
    }
}
