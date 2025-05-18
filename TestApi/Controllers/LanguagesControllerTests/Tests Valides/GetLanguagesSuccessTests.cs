using ArzenalApi.DTOs.LanguageDto;
using System.Text.Json;
using TestApi.Infrastructure;

namespace TestApi.Controllers.LanguagesControllerTests.Tests_Valides
{
    [Collection("SharedDatabaseCollection")]
    public class GetLanguagesSuccessTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;


        public GetLanguagesSuccessTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetLanguages_ReturnsList_WhenLanguagesExist()
        {
            // Arrange
            

            // Act
            var response = await _client.GetAsync("/api/languages");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            var languages = JsonSerializer.Deserialize<List<ReadLanguageDto>>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotEmpty(languages!);
            Assert.Contains(languages!, l => l.Name == "English");
            Assert.Contains(languages!, l => l.Name == "French");
        }

        [Fact]
        public async Task GetLanguagesById_ReturnsList_WhenIdIsValid()
        {
            // Arrange
            var id = 1;

            // Act
            var response = await _client.GetAsync($"/api/languages/{id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            var languages = JsonSerializer.Deserialize<ReadLanguageDto>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            
            Assert.Equal("English", languages!.Name);
        }



    }
}
