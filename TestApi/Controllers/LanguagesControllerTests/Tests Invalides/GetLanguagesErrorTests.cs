using Arzenal.Shared.Dtos.DTOs.LanguageDto;
using System.Net;
using System.Text.Json;
using TestApi.Infrastructure;

namespace TestApi.Controllers.LanguagesControllerTests.Tests_Invalides
{
    [Collection("SharedDatabaseCollection")]
    public class GetLanguagesErrorTests
    {

        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;


        public GetLanguagesErrorTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task GetLanguageById_ReturnsBadRequest_WhenIdIsunder(int invalidId)
        {
            // Arrange


            // Act
            var response = await _client.GetAsync($"/api/languages/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetLanguageById_ReturnNotFound_WhenIdIsInvalid()
        {
            // Arrange
            var invalidId = 999; // Nouvelle Id qui n'existe pas dans la base
            var response = await _client.GetAsync($"/api/languages/{invalidId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetLanguageById_ReturnBadrequest_WhenIdIsMalformed()
        {
            // Arrange
            var MalFormedId = "abc";
            var response = await _client.GetAsync($"/api/languages/{MalFormedId}");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetAllLanguage_ReturnsEmptyList_WhenNoLanguageExist()
        {
            // Arrange
            var (_, client2) = TestHelpers.CreateClientWithEmptyDb<Program>();
            var response = await client2.GetAsync("/api/languages");

            //Act
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var languages = JsonSerializer.Deserialize<List<ReadLanguageDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            //Assert
            Assert.NotNull(languages);
            Assert.Empty(languages);
        }

        [Fact]
        public async Task GetAllLanguages_ReturnsEmptyList_WhenNoAppsExist()
        {
            // Arrange
            var (_, client) = TestHelpers.CreateClientWithEmptyDb<Program>();

            // Act
            var response = await client.GetAsync("/api/languages");

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrEmpty(content), "Le contenu de la réponse ne doit pas être vide.");

            var apps = JsonSerializer.Deserialize<List<ReadLanguageDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(apps);
            Assert.Empty(apps);
        }
    }
}
