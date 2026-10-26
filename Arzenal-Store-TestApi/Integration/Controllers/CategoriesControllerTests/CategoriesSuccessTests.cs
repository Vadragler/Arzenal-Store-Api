using Arzenal.Dto.DTOs.CategorieDto;
using Arzenal.Store.Api.TestIntegration.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Arzenal.Store.Api.TestIntegration.Integration.Controllers.CategoriesControllerTests
{
    public class CategoriesSuccessTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public CategoriesSuccessTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateAuthenticatedClient();
        }

        [Fact]
        public async Task DeleteCategories_ReturnsNoContent_WhenLanguageExists()
        {
            // Arrange

            var dto = new CreateCategorieDto
            {
                Name = "Categorie to Delete",
            };
            var userId = Guid.Parse("d3f1a9e2-5b6c-4f7d-9a12-8c3e4b5f6789");
            var deviceName = "IntegrationTestDevice";

            // 1️⃣ Serialize le DTO une seule fois
            var jsonBody = JsonSerializer.Serialize(dto);

            // 2️⃣ Calculer signature
            var url = "api/categories/";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseJson);
            var id = responseObject.GetProperty("Id");
            // Act
            var deleteResponse = await _client.DeleteAsync($"api/categories/{id}");

            // Assert
            deleteResponse.EnsureSuccessStatusCode();

            // Vérifier si l'élément a été supprimé
            var getResponse = await _client.GetAsync($"api/categories/{id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }
    }
}
