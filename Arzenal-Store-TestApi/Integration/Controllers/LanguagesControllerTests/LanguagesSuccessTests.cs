using Arzenal.Dto.DTOs.AppDto;
using Arzenal.Dto.DTOs.LanguageDto;
using Arzenal.Store.Api.TestIntegration.Infrastructure;
using ArzenalStoreInfrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Arzenal.Store.Api.TestIntegration.Integration.Controllers.LanguagesControllerTests
{
    public class LanguagesControllerSuccessTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public LanguagesControllerSuccessTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateAuthenticatedClient();
        }

        // ------------------- GET -------------------
        [Fact]
        public async Task GetLanguages_ReturnsList_WhenLanguagesExist()
        {
            // Act
            var response = await _client.GetAsync("api/languages");

            // Assert
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var languages = JsonSerializer.Deserialize<List<ReadLanguageDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotEmpty(languages!);
            Assert.Contains(languages!, l => l.Name == "English");
            Assert.Contains(languages!, l => l.Name == "French");
        }

        [Fact]
        public async Task GetLanguageById_ReturnsLanguage_WhenIdIsValid()
        {
            // Arrange
            var id = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d");

            // Act
            var response = await _client.GetAsync($"api/languages/{id}");

            // Assert
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var language = JsonSerializer.Deserialize<ReadLanguageDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.Equal("English", language!.Name);
        }

        // ------------------- POST -------------------
        [Fact]
        public async Task PostLanguage_ReturnsCreated_WhenDataIsValid()
        {
            // Arrange
            var newLanguage = new CreateLanguageDto { Name = "Spanish" };
            var userId = Guid.Parse("d3f1a9e2-5b6c-4f7d-9a12-8c3e4b5f6789");
            var deviceName = "IntegrationTestDevice";

            // 1️⃣ Serialize le DTO une seule fois
            var jsonBody = JsonSerializer.Serialize(newLanguage);

            // 2️⃣ Calculer signature
            var url = $"api/languages/";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var postResponse = await _client.SendAsync(request);
            postResponse.EnsureSuccessStatusCode();

            // Assert
            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var createdLanguage = JsonSerializer.Deserialize<ReadLanguageDto>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var getResponse = await _client.GetAsync($"api/languages/{createdLanguage!.Id}");
            getResponse.EnsureSuccessStatusCode();

            var getJson = await getResponse.Content.ReadAsStringAsync();
            var fetchedLanguage = JsonSerializer.Deserialize<ReadLanguageDto>(getJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.Equal("Spanish", fetchedLanguage!.Name);
        }

        // ------------------- PUT -------------------
        [Fact]
        public async Task PutLanguage_ReturnsOk_WhenDataIsValid()
        {
            // Arrange
            var newLanguage = new CreateLanguageDto { Name = "Languages to Update" };
            var userId = Guid.Parse("d3f1a9e2-5b6c-4f7d-9a12-8c3e4b5f6789");
            var deviceName = "IntegrationTestDevice";

            // 1️⃣ Serialize le DTO une seule fois
            var jsonBody = JsonSerializer.Serialize(newLanguage);

            // 2️⃣ Calculer signature
            var url = $"api/languages/";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var postResponse = await _client.SendAsync(request);
            postResponse.EnsureSuccessStatusCode();

            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var createdLanguageId = JsonSerializer.Deserialize<JsonElement>(responseJson).GetProperty("Id");

            var updatedLanguage = new UpdateLanguageDto { Name = "Italian Updated" };

            // 1️⃣ Serialize le DTO une seule fois
            jsonBody = JsonSerializer.Serialize(updatedLanguage);

            // 2️⃣ Calculer signature
            url = $"api/languages/{createdLanguageId}";

            request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var putResponse = await _client.SendAsync(request);

            // Assert
            putResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync($"api/languages/{createdLanguageId}");
            getResponse.EnsureSuccessStatusCode();

            var updatedJson = await getResponse.Content.ReadAsStringAsync();
            var updatedLang = JsonSerializer.Deserialize<ReadLanguageDto>(updatedJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.Equal("Italian Updated", updatedLang!.Name);
        }

        // ------------------- DELETE -------------------
        [Fact]
        public async Task DeleteLanguage_ReturnsNoContent_WhenLanguageExists()
        {
            // Arrange
            var newLanguage = new CreateLanguageDto { Name = "Language to Delete" };
            var userId = Guid.Parse("d3f1a9e2-5b6c-4f7d-9a12-8c3e4b5f6789");
            var deviceName = "IntegrationTestDevice";

            // 1️⃣ Serialize le DTO une seule fois
            var jsonBody = JsonSerializer.Serialize(newLanguage);

            // 2️⃣ Calculer signature
            var url = $"api/languages/";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var postResponse = await _client.SendAsync(request);
            postResponse.EnsureSuccessStatusCode();

            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var createdLanguageId = JsonSerializer.Deserialize<JsonElement>(responseJson).GetProperty("Id");

            // Act
            var deleteResponse = await _client.DeleteAsync($"api/languages/{createdLanguageId}");

            // Assert
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteLanguage_Should_Remove_Associated_AppLanguages()
        {
            // Arrange
            var language = new CreateLanguageDto { Name = "Japan" };
            var userId = Guid.Parse("d3f1a9e2-5b6c-4f7d-9a12-8c3e4b5f6789");
            var deviceName = "IntegrationTestDevice";

            // 1️⃣ Serialize le DTO une seule fois
            var jsonBody = JsonSerializer.Serialize(language);

            // 2️⃣ Calculer signature
            var url = $"api/languages/";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var postResponse = await _client.SendAsync(request);
            postResponse.EnsureSuccessStatusCode();

            var languageResponse = JsonSerializer.Deserialize<ReadLanguageDto>(await postResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var languageId = languageResponse!.Id;
           
            var app = new CreateAppDto
            {
                Name = "New App 5",
                Version = "3.0.0",
                CategoryId = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"),
                LanguageIds = new List<Guid> { languageId }
            };


            // 1️⃣ Serialize le DTO une seule fois
            jsonBody = JsonSerializer.Serialize(app);

            // 2️⃣ Calculer signature
            url = $"api/apps/";

            request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var postAppResponse = await _client.SendAsync(request);
            postAppResponse.EnsureSuccessStatusCode();

            // Act
            var deleteResponse = await _client.DeleteAsync($"api/languages/{languageId}");
            deleteResponse.EnsureSuccessStatusCode();

            // Assert
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var exists = await context.AppLanguages.AnyAsync(al => al.LanguageId == languageId);
                Assert.False(exists);
            }
        }
    }
}
