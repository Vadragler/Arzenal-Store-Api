using Arzenal.Dto.DTOs.OperatingSystemDto;
using Arzenal.Store.Api.Service.Interfaces;
using ArzenalStoreApi.Controllers.Apps;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Controllers.OperatingSystemsControllerTests
{
    public class OperatingSystemsSuccessTests
    {
        private readonly Mock<IOperatingSystemService> _osServiceMock;
        private readonly OperatingSystemsController _controller;

        public OperatingSystemsSuccessTests()
        {
            _osServiceMock = new Mock<IOperatingSystemService>();
            _controller = new OperatingSystemsController(_osServiceMock.Object);
        }

        [Fact]
        public async Task GetAllOperatingSystems_ReturnsOk_WithList()
        {
            // Arrange
            var osList = new List<ReadOperatingSystemDto>
            {
                new ReadOperatingSystemDto { Id = Guid.NewGuid(), Name = "Windows" },
                new ReadOperatingSystemDto { Id = Guid.NewGuid(), Name = "Linux" }
            };

            _osServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(osList);

            // Act
            var result = await _controller.GetAllOperatingSystems();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<List<ReadOperatingSystemDto>>(okResult.Value);
            Assert.Equal(2, data.Count);
        }

        [Fact]
        public async Task GetOperatingSystemById_ReturnsOk()
        {
            // Arrange
            var id = Guid.NewGuid();
            var os = new ReadOperatingSystemDto { Id = id, Name = "macOS" };
            _osServiceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(os);

            // Act
            var result = await _controller.GetOperatingSystemById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<ReadOperatingSystemDto>(okResult.Value);
            Assert.Equal(id, data.Id);
        }

        [Fact]
        public async Task PostOperatingSystem_ReturnsCreatedAtAction()
        {
            // Arrange
            var createDto = new CreateOperatingSystemDto { Name = "Ubuntu" };
            var readDto = new ReadOperatingSystemDto { Id = Guid.NewGuid(), Name = "Ubuntu" };
            _osServiceMock.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(readDto);

            // Act
            var result = await _controller.PostOperatingSystem(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var data = Assert.IsType<ReadOperatingSystemDto>(createdResult.Value);
            Assert.Equal(readDto.Id, data.Id);
        }

        [Fact]
        public async Task PutOperatingSystem_ReturnsNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            var updateDto = new UpdateOperatingSystemDto { Name = "Fedora" };
            _osServiceMock.Setup(s => s.UpdateAsync(id, updateDto)).ReturnsAsync(true);

            // Act
            var result = await _controller.PutOperatingSystem(id, updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteOperatingSystem_ReturnsNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            _osServiceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteOperatingSystem(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
