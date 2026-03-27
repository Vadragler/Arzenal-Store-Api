using Arzenal.Dto.DTOs.AccountDto;
using Arzenal.Store.Api.Infrastructure.Data;
using Arzenal.Store.Api.TestIntegration.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace Arzenal.Store.Api.TestIntegration.Integration.Controllers.DeviceControllerTests
{
    public class DeviceControllerSuccessTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        [Fact]
        public async Task GetAllDevice_ReturnsOk_WithDevices()
        {
            // Arrange
            await using var factory = new CustomWebApplicationFactory<Program>();
            var client = factory.CreateAuthenticatedClient("Chrome");
            var dateNow = DateTime.UtcNow;

            // Act
            var response = await client.GetAsync("api/devices");

            // Assert
            response.EnsureSuccessStatusCode();
            var devices = await response.Content.ReadFromJsonAsync<List<ReadDeviceDto>>();
            Assert.NotEmpty(devices);
            var device = devices.First(d => d.DeviceName == "IntegrationTestDevice");
            Assert.Equal("IntegrationTestDevice", device.DeviceName);
            Assert.Equal("127.0.0.1", device.LastUsedIp);
            Assert.Equal(dateNow.ToString("m"), device.LastSeen.ToString("m"));
            Assert.True(device.IsActive);
            Assert.Equal("Chrome", device.Type);
            Assert.True(device.IsCurrentDevice);

        }

        [Fact]
        public async Task RevokeDevice_ReturnsOk_WhenDeviceExists()
        {
            await using var factory = new CustomWebApplicationFactory<Program>();
            var client = factory.CreateAuthenticatedClient("Chrome");
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

            var response = await client.DeleteAsync($"api/devices/revoke/a1b2c3d4-e5f6-4789-9012-3456789abcde");
            response.EnsureSuccessStatusCode();

            Guid token = Guid.Parse("a1b2c3d4-e5f6-4789-9012-3456789abcde");
            var updated = await db.RefreshTokens.FindAsync(token);
            Assert.Null(updated);
        }
    }
}
