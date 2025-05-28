using Arzenal.Shared.Dtos.DTOs.CategorieDto;
using System.Net;
using System.Text.Json;
using TestApi.Infrastructure;

namespace TestApi.Controllers.CategoriesControllerTests.Tests_Invalides
{
    [Collection("SharedDatabaseCollection")]
    public class GetCategoriesErrorTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public GetCategoriesErrorTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task GetCategorieById_ReturnsBadRequest_WhenIdIsunder(int invalidId)
        {
            // Arrange


            // Act
            var response = await _client.GetAsync($"/api/categories/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetCategorieById_ReturnNotFound_WhenIdIsInvalid()
        {
            // Arrange
            var invalidId = 999; // Nouvelle Id qui n'existe pas dans la base
            var response = await _client.GetAsync($"/api/categories/{invalidId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetCategorieById_ReturnBadrequest_WhenIdIsMalformed()
        {
            // Arrange
            var MalFormedId = "abc";
            var response = await _client.GetAsync($"/api/categories/{MalFormedId}");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetAllCategorie_ReturnsEmptyList_WhenNoLanguageExist()
        {
            // Arrange
            var (_, client2) = TestHelpers.CreateClientWithEmptyDb<Program>();
            var response = await client2.GetAsync("/api/categories");

            //Act
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var categorie = JsonSerializer.Deserialize<List<ReadCategorieDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            //Assert
            Assert.NotNull(categorie);
            Assert.Empty(categorie);
        }

        [Fact]
        public async Task GetAllCategories_ReturnsEmptyList_WhenNoAppsExist()
        {
            // Arrange
            var (_, client) = TestHelpers.CreateClientWithEmptyDb<Program>();

            // Act
            var response = await client.GetAsync("/api/categories");

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrEmpty(content), "Le contenu de la réponse ne doit pas être vide.");

            var apps = JsonSerializer.Deserialize<List<ReadCategorieDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(apps);
            Assert.Empty(apps);
        }

    }
}
