using ArzenalApi.DTOs.CategorieDto;
using System.Text;
using System.Text.Json;
using TestApi.Infrastructure;

namespace TestApi.Controllers.CategoriesControllerTests.Tests_Valides
{
    [Collection("SharedDatabaseCollection")]
    public class PostCategoriesSuccessTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        public PostCategoriesSuccessTests(CustomWebApplicationFactory<Program> factory) 
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task PostCategories_ReturnsCreated_WhenDataIsValid()
        {
            // Arrange
            var newCategorie = new CreateCategorieDto
            {
                Name = "Éducation111"
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(newCategorie),
                Encoding.UTF8,
                "application/json");

            // Act
            var postResponse = await _client.PostAsync("/api/categories", jsonContent);

            // Assert
            postResponse.EnsureSuccessStatusCode();
            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<ReadCategorieDto>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            var id = responseObject!.Id;

            // Vérifier que l'élément est bien ajouté
            var getResponse = await _client.GetAsync($"/api/categories/{id}");
            getResponse.EnsureSuccessStatusCode();
            var newCategorieJson = await getResponse.Content.ReadAsStringAsync();
            var createdCategorie = JsonSerializer.Deserialize<ReadCategorieDto>(newCategorieJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.Equal("Éducation111", createdCategorie!.Name);
        }

    }
}
