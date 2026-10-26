using Arzenal.Dto.DTOs.AppDto;
using Arzenal.Store.Api.TestIntegration.Infrastructure;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Arzenal.Store.Api.TestIntegration.Integration.Controllers.AppsControllerTests
{
    public class AppsControllerInvalidTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AppsControllerInvalidTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateAuthenticatedClient();
        }

        // ---------- GET ----------
        [Theory]
        [InlineData("abc", HttpStatusCode.BadRequest)]
        [InlineData("1", HttpStatusCode.BadRequest)]
        public async Task GetApp_InvalidIds_ReturnsBadRequest(string id, HttpStatusCode expected)
        {
            // Act
            var response = await _client.GetAsync($"api/apps/{id}");

            // Assert
            Assert.Equal(expected, response.StatusCode);
        }

        [Fact]
        public async Task GetApp_NotFound_Returns404()
        {
            // Act
            var response = await _client.GetAsync($"api/apps/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ---------- POST ----------
        [Theory]
        [InlineData(null, "1.0", "The Name field is required.")]
        [InlineData("AppName", null, "The Version field is required.")]
        public async Task PostApp_MissingFields_ReturnsBadRequest(string? name, string? version, string expectedMessage)
        {
            // Arrange
            var dto = new CreateAppDto
            {
                Name = name!,
                Version = version!,
                Description = "Test",
                IsVisible = true,
                IconePath = "C:/test.png",
                Requirements = "Win10",
                AppSize = 100,
                LastUpdated = DateTime.UtcNow,
                CategoryId = Guid.NewGuid()
            };

            var userId = Guid.Parse("d3f1a9e2-5b6c-4f7d-9a12-8c3e4b5f6789");
            var deviceName = "IntegrationTestDevice";

            // 1️⃣ Serialize le DTO une seule fois
            var jsonBody = JsonSerializer.Serialize(dto);

            // 2️⃣ Calculer signature
            var url = $"api/apps/";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(expectedMessage, await response.Content.ReadAsStringAsync());
        }

        // ---------- PATCH ----------
        [Theory]
        [InlineData("abc", HttpStatusCode.BadRequest)]
        [InlineData("1", HttpStatusCode.BadRequest)]
        public async Task PatchApp_InvalidIds_ReturnsBadRequest(string id, HttpStatusCode expected)
        {
            // Arrange
            var dto = new UpdateAppDto { Name = "Test", Version = "1.0" };
            var userId = Guid.Parse("d3f1a9e2-5b6c-4f7d-9a12-8c3e4b5f6789");
            var deviceName = "IntegrationTestDevice";

            // 1️⃣ Serialize le DTO une seule fois
            var jsonBody = JsonSerializer.Serialize(dto);

            // 2️⃣ Calculer signature
            var url = $"api/apps/{id}";

            var request = new HttpRequestMessage(HttpMethod.Patch, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(expected, response.StatusCode);
        }

        [Fact]
        public async Task PatchApp_NotFound_Returns404()
        {
            // Arrange
            var dto = new UpdateAppDto { Name = "Test", Version = "1.0"};
            // 1️⃣ Serialize le DTO une seule fois
            var userId = Guid.Parse("d3f1a9e2-5b6c-4f7d-9a12-8c3e4b5f6789");
            var deviceName = "IntegrationTestDevice";

            // 1️⃣ Serialize le DTO une seule fois
            var jsonBody = JsonSerializer.Serialize(dto);

            // 2️⃣ Calculer signature
            var url = $"api/apps/{Guid.NewGuid()}";

            var request = new HttpRequestMessage(HttpMethod.Patch, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ---------- DELETE ----------
        [Theory]
        [InlineData("abc", HttpStatusCode.BadRequest)]
        [InlineData("1", HttpStatusCode.BadRequest)]
        public async Task DeleteApp_InvalidIds_ReturnsBadRequest(string id, HttpStatusCode expected)
        {
            // Act
            var response = await _client.DeleteAsync($"api/apps/{id}");

            // Assert
            Assert.Equal(expected, response.StatusCode);
        }

        [Fact]
        public async Task DeleteApp_NotFound_Returns404()
        {
            // Act
            var response = await _client.DeleteAsync($"api/apps/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
