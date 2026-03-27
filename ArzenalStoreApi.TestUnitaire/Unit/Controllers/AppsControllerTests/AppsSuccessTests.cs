using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Dto.DTOs.AppDto;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ArzenalStoreApi.Controllers.Apps;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Controllers.AppsControllerTests
{
    public class AppsSuccessTests
    {
        private readonly Mock<IAppService> _appServiceMock;
        private readonly AppsController _controller;

        public AppsSuccessTests()
        {
            _appServiceMock = new Mock<IAppService>();
            _controller = new AppsController(_appServiceMock.Object);
        }

        [Fact]
        public async Task GetAllApps_ReturnsOk_WithList()
        {
            // Arrange
            var apps = new List<ReadAppDto>
            {
                new ReadAppDto { Id = Guid.NewGuid(), Name = "App1" },
                new ReadAppDto { Id = Guid.NewGuid(), Name = "App2" }
            };
            _appServiceMock.Setup(s => s.GetAllAppsAsync()).ReturnsAsync(apps);

            // Act
            var result = await _controller.GetAllApps();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<List<ReadAppDto>>(okResult.Value);
            Assert.Equal(2, data.Count);
        }

        [Fact]
        public async Task GetAppById_ReturnsOk_WithApp()
        {
            // Arrange
            var appId = Guid.NewGuid();
            var appDto = new ReadAppDto { Id = appId, Name = "TestApp" };
            _appServiceMock.Setup(s => s.GetAppByIdAsync(appId)).ReturnsAsync(appDto);

            // Act
            var result = await _controller.GetAppById(appId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<ReadAppDto>(okResult.Value);
            Assert.Equal(appId, data.Id);
        }

        [Fact]
        public async Task CreateApp_ReturnsCreatedAtAction()
        {
            // Arrange
            var createDto = new CreateAppDto { Name = "NewApp", CategoryId = Guid.NewGuid(),Version = "Alpha" };
            var readDto = new ReadAppDto { Id = Guid.NewGuid(), Name = "NewApp" };
            _appServiceMock.Setup(s => s.CreateAppAsync(createDto)).ReturnsAsync(readDto);

            // Act
            var result = await _controller.CreateApp(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var data = Assert.IsType<ReadAppDto>(createdResult.Value);
            Assert.Equal(readDto.Id, data.Id);
        }

        [Fact]
        public async Task UpdateApp_ReturnsOk_WithDto()
        {
            // Arrange
            var id = Guid.NewGuid();
            var updateDto = new UpdateAppDto { Name = "Updated" };
            var readDto = new ReadAppDto { Id = id, Name = "Updated" };
            _appServiceMock.Setup(s => s.UpdateAppAsync(id, updateDto)).ReturnsAsync(readDto);

            // Act
            var result = await _controller.UpdateApp(id, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<ReadAppDto>(okResult.Value);
            Assert.Equal("Updated", data.Name);
        }

        [Fact]
        public async Task DeleteApp_ReturnsNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            _appServiceMock.Setup(s => s.DeleteAppAsync(id)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteApp(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
