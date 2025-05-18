using Arzenal.Shared.Dtos.DTOs.CategorieDto;
using System.Net;
using System.Text;
using System.Text.Json;
using TestApi.Infrastructure;

namespace TestApi.Controllers.CategoriesControllerTests.Tests_Valides
{
    [Collection("SharedDatabaseCollection")]
    public class DeleteCategoriesSuccessTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public DeleteCategoriesSuccessTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
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

            var postResponse = await _client.PostAsync("/api/categories", jsonContent);
            postResponse.EnsureSuccessStatusCode();
            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseJson);
            var id = responseObject.GetProperty("id");
            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/categories/{id}");

            // Assert
            deleteResponse.EnsureSuccessStatusCode();

            // Vérifier si l'élément a été supprimé
            var getResponse = await _client.GetAsync($"/api/categories/{id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }
    }
}
