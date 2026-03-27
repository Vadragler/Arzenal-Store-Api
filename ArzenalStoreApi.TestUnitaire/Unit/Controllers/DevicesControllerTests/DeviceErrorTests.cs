using Arzenal.Store.Api.Service.Interfaces;
using ArzenalStoreApi.Controllers.User;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Controllers.DevicesControllerTests
{
    public class DeviceErrorTests
    {
        private readonly Mock<IDeviceService> _deviceServiceMock;
        private readonly DevicesController _deviceController;

        public DeviceErrorTests()
        {
            _deviceServiceMock = new Mock<IDeviceService>();
            _deviceController = new DevicesController(_deviceServiceMock.Object);
        }

        [Fact]
        public async Task GetAllDevice_Throws_WhenServiceFails()
        {
            // Arrange
            _deviceServiceMock.Setup(s => s.GetAllDevice(It.IsAny<HttpRequest>()))
                .ThrowsAsync(new Exception("Erreur récupération"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(_deviceController.GetAllDevice);
        }

        [Fact]
        public async Task RevokeDevice_Throws_WhenServiceFails()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            _deviceServiceMock.Setup(s => s.RevokeDevice(deviceId))
                .ThrowsAsync(new UnauthorizedAccessException("Accès refusé"));

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _deviceController.RevokeDevice(deviceId));
        }
    }
}
