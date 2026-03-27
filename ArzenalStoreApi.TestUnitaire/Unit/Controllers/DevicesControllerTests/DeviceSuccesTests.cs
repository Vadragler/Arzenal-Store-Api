using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Dto.DTOs.AccountDto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ArzenalStoreApi.Controllers.User;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Controllers.DevicesControllerTests
{
    public class DeviceSuccesTests
    {
        private readonly Mock<IDeviceService> _deviceServiceMock;
        private readonly DevicesController _deviceController;

        public DeviceSuccesTests()
        {
            _deviceServiceMock = new Mock<IDeviceService>();
            _deviceController = new DevicesController(_deviceServiceMock.Object);
        }

        [Fact]
        public async Task GetAllDevice_ReturnsOk_WithDevices()
        {
            // Arrange
            var devices = new List<ReadDeviceDto>
            {
                new ReadDeviceDto { Id = Guid.NewGuid(), DeviceName = "PC1" },
                new ReadDeviceDto { Id = Guid.NewGuid(), DeviceName = "Phone1" }
            };
            _deviceServiceMock.Setup(s => s.GetAllDevice(It.IsAny<HttpRequest>()))
                .ReturnsAsync(devices);

            // Act
            var result = await _deviceController.GetAllDevice();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<ReadDeviceDto>>(okResult.Value);
            Assert.Equal(2, data.Count);
        }

        [Fact]
        public async Task RevokeDevice_ReturnsOk_WhenSuccess()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            _deviceServiceMock.Setup(s => s.RevokeDevice(deviceId))
                .ReturnsAsync(true);

            // Act
            var result = await _deviceController.RevokeDevice(deviceId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, bool>>(json);
            Assert.True(data["success"]);

        }
    }
}
