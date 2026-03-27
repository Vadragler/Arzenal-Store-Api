using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Dto.DTOs.AppDto;
using Moq;
using ArzenalStoreApi.Controllers.Apps;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Controllers.AppsControllerTests
{
    public class AppsErrorTests
    {
        private readonly Mock<IAppService> _appServiceMock;
        private readonly AppsController _controller;

        public AppsErrorTests()
        {
            _appServiceMock = new Mock<IAppService>();
            _controller = new AppsController(_appServiceMock.Object);
        }

        [Fact]
        public async Task GetAllApps_ThrowsException()
        {
            // Arrange
            _appServiceMock.Setup(s => s.GetAllAppsAsync())
                .ThrowsAsync(new Exception("Erreur récupération"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.GetAllApps());
            Assert.Equal("Erreur récupération", ex.Message);
        }

        [Fact]
        public async Task GetAppById_ThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _appServiceMock.Setup(s => s.GetAppByIdAsync(id))
                .ThrowsAsync(new Exception("App introuvable"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.GetAppById(id));
            Assert.Equal("App introuvable", ex.Message);
        }

        [Fact]
        public async Task CreateApp_ThrowsException()
        {
            // Arrange
            var createDto = new CreateAppDto { Name = "NewApp", CategoryId = Guid.NewGuid(), Version = "Alpha" };
            _appServiceMock.Setup(s => s.CreateAppAsync(createDto))
                .ThrowsAsync(new Exception("Erreur création"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.CreateApp(createDto));
            Assert.Equal("Erreur création", ex.Message);
        }

        [Fact]
        public async Task UpdateApp_ThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var updateDto = new UpdateAppDto { Name = "Updated" };
            _appServiceMock.Setup(s => s.UpdateAppAsync(id, updateDto))
                .ThrowsAsync(new Exception("Erreur update"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.UpdateApp(id, updateDto));
            Assert.Equal("Erreur update", ex.Message);
        }

        [Fact]
        public async Task DeleteApp_ThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _appServiceMock.Setup(s => s.DeleteAppAsync(id))
                .ThrowsAsync(new Exception("Erreur suppression"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.DeleteApp(id));
            Assert.Equal("Erreur suppression", ex.Message);
        }
    }
}
