using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.TestIntegration.Infrastructure;
using System.Net;

namespace Arzenal.Store.Api.TestIntegration.Integration.Controllers.DeviceControllerTests
{
    public class DeviceControllerErrorTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        public DeviceControllerErrorTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateAuthenticatedClient("Chrome");
        }

        [Fact]
        public async Task RevokeDevice_ReturnsUnauthorized_WhenNoUser()
        {
            // Arrange : on crée un client sans JWT ou cookie valide
            var anonClient = _factory.CreateClient(); // client non authentifié

            // Act
            var response = await anonClient.DeleteAsync($"api/devices/revoke/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task RevokeDevice_ReturnsNotFound_WhenDeviceDoesNotExist()
        {
            // Arrange : utilisateur authentifié mais device inexistant
            var fakeDeviceId = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"api/devices/revoke/{fakeDeviceId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
