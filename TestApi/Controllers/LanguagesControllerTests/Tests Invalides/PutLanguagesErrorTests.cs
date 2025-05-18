using Arzenal.Shared.Dtos.DTOs.LanguageDto;
using System.Net;
using System.Text;
using System.Text.Json;
using TestApi.Infrastructure;

namespace TestApi.Controllers.LanguagesControllerTests.Tests_Invalides
{
    [Collection("SharedDatabaseCollection")]
    public class PutLanguagesErrorTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;


        public PutLanguagesErrorTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task PutLanguage_ReturnBadRequest_WhenLanguageAlreadyExist()
        {
            // Arrange
            var newLanguage = new CreateLanguageDto { Name = "Italian" };
            var jsonContent = new StringContent(
               JsonSerializer.Serialize(newLanguage),
               Encoding.UTF8,
               "application/json");
            // Act
            var postResponse = await _client.PostAsync("/api/languages", jsonContent);
            postResponse.EnsureSuccessStatusCode();
            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<ReadLanguageDto>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            var id = responseObject!.Id;
            var updatedLanguage = new UpdateLanguageDto { Name = "French" };
            jsonContent = new StringContent(
                JsonSerializer.Serialize(updatedLanguage),
                Encoding.UTF8,
                "application/json");
            var putResponse = await _client.PutAsync($"/api/languages/{id}", jsonContent);
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, putResponse.StatusCode);
        }

        [Fact]
        public async Task PutLanguage_ReturnBadRequest_WhenIdIsMalformed()
        {
            var updateDto = new UpdateLanguageDto
            {
                Name = "New Name",
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(updateDto),
                Encoding.UTF8,
                "application/json");
            var id = "abc";
            var response = await _client.PutAsync($"/api/languages/{id}", jsonContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task PutLanguage_ReturnBadRequest_WhenIdIsUnder(int id)
        {
            var updateDto = new UpdateLanguageDto
            {
                Name = "New Name",
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(updateDto),
                Encoding.UTF8,
                "application/json");
            var response = await _client.PutAsync($"/api/languages/{id}", jsonContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutLanguage_ReturnNotFound_WhenIdDoesNotExist()
        {
            var updateDto = new UpdateLanguageDto
            {
                Name = "New Name",
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(updateDto),
                Encoding.UTF8,
                "application/json");
            var id = 999;
            var response = await _client.PutAsync($"/api/languages/{id}", jsonContent);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

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
        public async Task PutLanguage_ReturnBadRequest_WhenFieldIsEmptyorNull(string name)
        {
            var newLanguage = new CreateLanguageDto
            {
                Name = "Nom vide"
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
            var updateDto = new UpdateLanguageDto
            {
                Name = name,
            };

            jsonContent = new StringContent(
                JsonSerializer.Serialize(updateDto),
                Encoding.UTF8,
                "application/json");
            
            var response = await _client.PutAsync($"/api/languages/{id}", jsonContent);
            var deleteResponse = await _client.DeleteAsync($"/api/languages/{id}");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

    }
}
