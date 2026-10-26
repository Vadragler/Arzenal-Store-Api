using Arzenal.Store.Api.TestIntegration.Infrastructure;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Arzenal.Store.Api.TestIntegration.Integration.Controllers.CategoriesControllerTests
{
    public class CategoriesControllerErrorTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public CategoriesControllerErrorTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateAuthenticatedClient();
        }

        // ---------- GET Tests ----------

        [Fact]
        public async Task GetCategorieById_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"api/categories/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetCategorieById_ReturnsBadRequest_WhenIdIsMalformed()
        {
            // Act
            var response = await _client.GetAsync("api/categories/abc");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ---------- DELETE Tests ----------

        [Fact]
        public async Task DeleteCategorie_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"api/categories/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCategorie_ReturnsBadRequest_WhenIdIsMalformed()
        {
            // Act
            var response = await _client.DeleteAsync("api/categories/123456");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ---------- POST Tests ----------

        [Fact]
        public async Task PostCategorie_ReturnBadRequest_WhenNameIsEmpty()
        {
            // Arrange
            var invalidDto = new { Name = "" }; // Champ Name vide
            var jsonBody = JsonSerializer.Serialize(invalidDto);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var userId = Guid.Parse("d3f1a9e2-5b6c-4f7d-9a12-8c3e4b5f6789");
            var deviceName = "IntegrationTestDevice";

            // Calculer signature HMAC

            var request = new HttpRequestMessage(HttpMethod.Post, "api/categories/")
            {
                Content = content
            };
            request.Headers.Add("X-Device-Name", deviceName);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            // Ici on attend que la validation du DTO échoue côté API (400 Bad Request)
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ---------- PUT Tests ----------
        [Fact]
        public async Task PutCategorie_ReturnBadRequest_WhenBodyIsMissing()
        {
            // Arrange
            var getresponse = await _client.GetAsync("api/categories");
            var responseJson = await getresponse.Content.ReadAsStringAsync();
            var responseArray = JsonSerializer.Deserialize<JsonElement>(responseJson);
            var id = responseArray[0].GetProperty("Id");

            var jsonContent = new StringContent("", Encoding.UTF8, "application/json");

            var userId = Guid.Parse("d3f1a9e2-5b6c-4f7d-9a12-8c3e4b5f6789");
            var deviceName = "IntegrationTestDevice";

            // 1️⃣ Serialize le DTO une seule fois
            var jsonBody = JsonSerializer.Serialize(jsonContent);

            // 2️⃣ Calculer signature
            var url = $"api/categories/{id}";

            var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

    }
}
