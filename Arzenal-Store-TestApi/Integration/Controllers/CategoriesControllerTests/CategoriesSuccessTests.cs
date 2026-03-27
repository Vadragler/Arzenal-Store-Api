using Arzenal.Dto.DTOs.CategorieDto;
using System.Net;
using System.Text;
using System.Text.Json;
using Arzenal.Store.Api.TestIntegration.Infrastructure;

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
            var newCategorie = new CreateCategorieDto
            {
                Name = "Categorie to Delete",
            };
            // Créer l'application via POST pour avoir un élément à supprimer
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(newCategorie),
                Encoding.UTF8,
                "application/json");

            var postResponse = await _client.PostAsync("api/categories", jsonContent);
            postResponse.EnsureSuccessStatusCode();
            var responseJson = await postResponse.Content.ReadAsStringAsync();
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
