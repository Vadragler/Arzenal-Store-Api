using ArzenalApi.DTOs.LanguageDto;
using System.Text;
using System.Text.Json;
using TestApi.Infrastructure;

namespace TestApi.Controllers.LanguagesControllerTests.Tests_Valides
{
    [Collection("SharedDatabaseCollection")]
    public class PostLanguagesSuccessTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;


        public PostLanguagesSuccessTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task PostLanguage_ReturnsCreated_WhenDataIsValid()
        {
            // Arrange
            var newLanguage = new CreateLanguageDto
            {
                Name = "Spanish"
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(newLanguage),
                Encoding.UTF8,
                "application/json");

            // Act
            var postResponse = await _client.PostAsync("/api/languages", jsonContent);

            // Assert
            postResponse.EnsureSuccessStatusCode();
            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<ReadLanguageDto>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            var id = responseObject!.Id;

            // Vérifier que l'élément est bien ajouté
            var getResponse = await _client.GetAsync($"/api/languages/{id}");
            getResponse.EnsureSuccessStatusCode();
            var newLanguageJson = await getResponse.Content.ReadAsStringAsync();
            var createdLanguage = JsonSerializer.Deserialize<ReadLanguageDto>(newLanguageJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.Equal("Spanish", createdLanguage!.Name);
        }

    }
}
