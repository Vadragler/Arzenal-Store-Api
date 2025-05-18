using ArzenalApi.DTOs.LanguageDto;
using System.Text;
using System.Text.Json;
using TestApi.Infrastructure;

namespace TestApi.Controllers.LanguagesControllerTests.Tests_Valides
{

    [Collection("SharedDatabaseCollection")]
    public class PutLanguagesSuccessTests
    {

        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;


        public PutLanguagesSuccessTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }
        [Fact]
        public async Task PutLanguage_ReturnsOk_WhenDataIsValid()
        {
            // Arrange
            var newLanguage = new CreateLanguageDto
            {
                Name = "Languages to Update",
            };
            // Créer l'application via POST pour avoir un élément à supprimer
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(newLanguage),
                Encoding.UTF8,
                "application/json");

            var postResponse = await _client.PostAsync("/api/languages", jsonContent);
            postResponse.EnsureSuccessStatusCode();
            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseJson);
            var id = responseObject.GetProperty("id");

            var updatedLanguage = new UpdateLanguageDto
            {
                Name = "Italian Updated"
            };

            jsonContent = new StringContent(
                JsonSerializer.Serialize(updatedLanguage),
                Encoding.UTF8,
                "application/json");

            // Act
            var putResponse = await _client.PutAsync($"/api/languages/{id}", jsonContent);

            // Assert
            putResponse.EnsureSuccessStatusCode();

            // Vérifier la mise à jour
            var getResponse = await _client.GetAsync($"/api/languages/{id}");
            getResponse.EnsureSuccessStatusCode();
            var updatedLanguageJson = await getResponse.Content.ReadAsStringAsync();
            var updatedLang = JsonSerializer.Deserialize<ReadLanguageDto>(updatedLanguageJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });


            Assert.Equal("Italian Updated", updatedLang!.Name);
        }

    }
}
