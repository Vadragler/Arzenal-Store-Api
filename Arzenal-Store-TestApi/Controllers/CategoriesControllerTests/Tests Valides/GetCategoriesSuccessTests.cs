using Arzenal.Shared.Dtos.DTOs.CategorieDto;
using System.Text.Json;
using TestApi.Infrastructure;

namespace TestApi.Controllers.CategoriesControllerTests.Tests_Valides
{
    [Collection("SharedDatabaseCollection")]
    public class GetCategoriesSuccessTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;


        public GetCategoriesSuccessTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }
        [Fact]
        public async Task GetCategories_ReturnsList_WhenCategoriesExist()
        {
            var response = await _client.GetAsync("/api/categories");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var categories = JsonSerializer.Deserialize<List<ReadCategorieDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(categories);
            Assert.NotEmpty(categories);
            Assert.Contains(categories, c => c.Name == "Software");
        }

        [Fact]
        public async Task GetCategoriesById_ReturnsList_WhenIdIsValid()
        {
            // Arrange
            var id = 1;

            // Act
            var response = await _client.GetAsync($"/api/categories/{id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            var categorie = JsonSerializer.Deserialize<ReadCategorieDto>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });


            Assert.Equal("Software", categorie!.Name);
        }


    }
}
