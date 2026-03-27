using Arzenal.Dto.DTOs.LanguageDto;
using Arzenal.Store.Api.TestIntegration.Infrastructure;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Arzenal.Store.Api.TestIntegration.Integration.Controllers.LanguagesControllerTests
{
    public class LanguagesControllerErrorTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public LanguagesControllerErrorTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateAuthenticatedClient();
        }

        // ------------------- GET -------------------
        [Fact]
        public async Task GetLanguageById_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"api/languages/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetLanguageById_ReturnsBadRequest_WhenIdIsMalformed()
        {
            // Act
            var response = await _client.GetAsync("api/languages/abc");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetAllLanguages_ReturnsEmptyList_WhenNoLanguagesExist()
        {
            // Act
            var response = await _client.GetAsync("api/languages");

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ------------------- POST -------------------
        public static IEnumerable<object[]> LongStrings =>
            new List<object[]>
            {
                new object[] { new string('A', 101) },
                new object[] { "" },
                new object[] { null! },
                new object[] { " " }
            };

        [Theory]
        [MemberData(nameof(LongStrings))]
        public async Task PostLanguages_ReturnsBadRequest_WhenDataIsInvalid(string name)
        {
            // Arrange
            var newLanguage = new CreateLanguageDto { Name = name };
            var jsonContent = new StringContent(JsonSerializer.Serialize(newLanguage), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("api/languages", jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ------------------- PUT -------------------
        [Fact]
        public async Task PutLanguage_ReturnsConflict_WhenLanguageAlreadyExist()
        {
            // Arrange
            var newLanguage = new CreateLanguageDto { Name = "Italian" };
            var jsonContent = new StringContent(JsonSerializer.Serialize(newLanguage), Encoding.UTF8, "application/json");

            // Act
            var postResponse = await _client.PostAsync("api/languages", jsonContent);
            postResponse.EnsureSuccessStatusCode();

            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var createdLanguage = JsonSerializer.Deserialize<ReadLanguageDto>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var updateDto = new UpdateLanguageDto { Name = "French" };
            jsonContent = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");
            var putResponse = await _client.PutAsync($"api/languages/{createdLanguage!.Id}", jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, putResponse.StatusCode);
        }

        [Fact]
        public async Task PutLanguage_ReturnsBadRequest_WhenIdIsMalformed()
        {
            // Arrange
            var updateDto = new UpdateLanguageDto { Name = "New Name" };
            var jsonContent = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync("api/languages/abc", jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutLanguage_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var updateDto = new UpdateLanguageDto { Name = "New Name" };
            var jsonContent = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"api/languages/{Guid.NewGuid()}", jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(LongStrings))]
        public async Task PutLanguage_ReturnsBadRequest_WhenFieldIsEmptyOrNull(string name)
        {
            // Arrange
            var newLanguage = new CreateLanguageDto { Name = "Temp" };
            var jsonContent = new StringContent(JsonSerializer.Serialize(newLanguage), Encoding.UTF8, "application/json");

            // Act
            var postResponse = await _client.PostAsync("api/languages", jsonContent);
            postResponse.EnsureSuccessStatusCode();

            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var createdLanguage = JsonSerializer.Deserialize<ReadLanguageDto>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var updateDto = new UpdateLanguageDto { Name = name };
            jsonContent = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");
            var putResponse = await _client.PutAsync($"api/languages/{createdLanguage!.Id}", jsonContent);

            var deleteResponse = await _client.DeleteAsync($"api/languages/{createdLanguage.Id}");

            // Assert
            deleteResponse.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.BadRequest, putResponse.StatusCode);
        }

        // ------------------- DELETE -------------------
        [Fact]
        public async Task DeleteLanguage_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Act
            var response = await _client.DeleteAsync($"api/languages/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteLanguage_ReturnsBadRequest_WhenIdIsMalformed()
        {
            // Act
            var response = await _client.DeleteAsync("api/languages/123456");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
