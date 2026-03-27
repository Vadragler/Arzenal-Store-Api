using Arzenal.Dto.DTOs.AppDto;
using System.Net;
using System.Text;
using System.Text.Json;
using Arzenal.Store.Api.TestIntegration.Infrastructure;

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

            var json = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("api/apps", json);

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
            var json = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PatchAsync($"api/apps/{id}", json);

            // Assert
            Assert.Equal(expected, response.StatusCode);
        }

        [Fact]
        public async Task PatchApp_NotFound_Returns404()
        {
            // Arrange
            var dto = new UpdateAppDto { Name = "Test", Version = "1.0" };
            var json = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PatchAsync($"api/apps/{Guid.NewGuid()}", json);

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
