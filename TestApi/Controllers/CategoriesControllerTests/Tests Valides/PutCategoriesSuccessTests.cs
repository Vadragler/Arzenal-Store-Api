using ArzenalApi.DTOs.CategorieDto;
using System.Text;
using System.Text.Json;
using TestApi.Infrastructure;

namespace TestApi.Controllers.CategoriesControllerTests.Tests_Valides
{
    [Collection("SharedDatabaseCollection")]
    public class PutCategoriesSuccessTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        public PutCategoriesSuccessTests(CustomWebApplicationFactory<Program> factory) 
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task PutCategoriesSuccess_ReturnsOk_WhenDataIsValid() 
        {
            // Arrange
            var newCategorie = new CreateCategorieDto
            {
                Name = "Categorie to Update1111111",
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

            var updatedCategorie = new UpdateCategorieDto
            {
                Name = "Categorie Updated11111111111111"
            };

            jsonContent = new StringContent(
                JsonSerializer.Serialize(updatedCategorie),
                Encoding.UTF8,
                "application/json");

            // Act
            var putResponse = await _client.PutAsync($"/api/categories/{id}", jsonContent);

            // Assert
            putResponse.EnsureSuccessStatusCode();

            // Vérifier la mise à jour
            var getResponse = await _client.GetAsync($"/api/categories/{id}");
            getResponse.EnsureSuccessStatusCode();
            var updatedCategorieJson = await getResponse.Content.ReadAsStringAsync();
            var updatedCat = JsonSerializer.Deserialize<ReadCategorieDto>(updatedCategorieJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });


            Assert.Equal("Categorie Updated11111111111111", updatedCat!.Name);
        }
    }
}
