using ArzenalApi.Data;
using ArzenalApi.DTOs.AppDto;
using ArzenalApi.DTOs.LanguageDto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.Text.Json;
using TestApi.Infrastructure;

namespace TestApi.Controllers.LanguagesControllerTests.Tests_Valides
{
    [Collection("SharedDatabaseCollection")]
    public class DeleteLanguagesSuccessTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;


        public DeleteLanguagesSuccessTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task DeleteLanguage_Should_Remove_Associated_AppLanguages()
        {
            // Arrange
            var language = new CreateLanguageDto { Name = "Japan" };
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(language),
                Encoding.UTF8,
                "application/json");

            // Créer la langue via l'API
            var postResponse = await _client.PostAsync("/api/languages", jsonContent);
            postResponse.EnsureSuccessStatusCode();

            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var languageResponse = JsonSerializer.Deserialize<ReadLanguageDto>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            int languageId = (int)(languageResponse?.Id);

            // Créer une application associée à cette langue
            var app = new CreateAppDto
            {
                Name = "New App 5",
                Version = "3.0.0",
                FilePath = "C:/PathToApp",
                CategoryId = 1,        
                LanguageIds = new List<int> { languageId }
            };

            jsonContent = new StringContent(
                JsonSerializer.Serialize(app),
                Encoding.UTF8,
                "application/json");

            var postAppResponse = await _client.PostAsync("/api/apps", jsonContent);
            postAppResponse.EnsureSuccessStatusCode();

            // Act: Supprimer la langue
            var deleteLanguageResponse = await _client.DeleteAsync($"/api/languages/{languageId}");
            deleteLanguageResponse.EnsureSuccessStatusCode();

            // Assert: Vérifier que la relation AppLanguage a bien été supprimée
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var appLanguageExists = await context.AppLanguages.AnyAsync(al => al.LanguageId == languageId);
                Assert.False(appLanguageExists);  // La relation devrait être supprimée
            }
        }


    

        [Fact]
        public async Task DeleteLanguage_ReturnsNoContent_WhenLanguageExists()
        {
            // Arrange
            var newLanguage = new CreateLanguageDto
            {
                Name = "Languages to Delete",
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
            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/languages/{id}");

            // Assert
            deleteResponse.EnsureSuccessStatusCode();

            // Vérifier si l'élément a été supprimé
            var getResponse = await _client.GetAsync($"/api/languages/{id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

    }
}
