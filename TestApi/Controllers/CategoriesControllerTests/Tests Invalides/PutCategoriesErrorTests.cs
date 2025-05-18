using ArzenalApi.DTOs.CategorieDto;
using System.Net;
using System.Text;
using System.Text.Json;
using TestApi.Infrastructure;

namespace TestApi.Controllers.CategoriesControllerTests.Tests_Invalides
{
    [Collection("SharedDatabaseCollection")]
    public class PutCategoriesErrorTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public PutCategoriesErrorTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task PutCategorie_ReturnBadRequest_WhenCategorieAlreadyExist()
        {
            // Arrange
            var newCategorie = new CreateCategorieDto { Name = "11111" };
            var jsonContent = new StringContent(
               JsonSerializer.Serialize(newCategorie),
               Encoding.UTF8,
               "application/json");
            // Act
            var postResponse = await _client.PostAsync("/api/categories", jsonContent);
            postResponse.EnsureSuccessStatusCode();
            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<ReadCategorieDto>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            var id = responseObject!.Id;
            var updatedCategorie = new UpdateCategorieDto { Name = "Software" };
            jsonContent = new StringContent(
                JsonSerializer.Serialize(updatedCategorie),
                Encoding.UTF8,
                "application/json");
            var putResponse = await _client.PutAsync($"/api/categories/{id}",jsonContent);
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, putResponse.StatusCode);
        }

        [Fact]
        public async Task PutCategorie_ReturnBadRequest_WhenIdIsMalformed()
        {
            var updateDto = new UpdateCategorieDto
            {
                Name = "New Name",
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(updateDto),
                Encoding.UTF8,
                "application/json");
            var id = "abc";
            var response = await _client.PutAsync($"/api/categories/{id}", jsonContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task PutCategorie_ReturnBadRequest_WhenIdIsUnder(int id)
        {
            var updateDto = new UpdateCategorieDto
            {
                Name = "New Name",
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(updateDto),
                Encoding.UTF8,
                "application/json");
            var response = await _client.PutAsync($"/api/categories/{id}", jsonContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutCategorie_ReturnNotFound_WhenIdDoesNotExist()
        {
            var updateDto = new UpdateCategorieDto
            {
                Name = "New Name",
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(updateDto),
                Encoding.UTF8,
                "application/json");
            var id = 999;
            var response = await _client.PutAsync($"/api/categories/{id}", jsonContent);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        public static IEnumerable<object[]> LongStrings =>
    new List<object[]>
    {
        new object[] {  new string('A', 101) },
        new object[] {  "" },
        new object[] {  null! },
        new object[] {  " " }
    };

        [Theory]
        [MemberData(nameof(LongStrings))]
        public async Task PutCategorie_ReturnBadRequest_WhenFieldIsEmptyorNull( string name)
        {
            var newLanguage = new CreateCategorieDto
            {
                Name = "Nom vide"
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(newLanguage),
                Encoding.UTF8,
                "application/json");

            // Act
            var postResponse = await _client.PostAsync("/api/categories", jsonContent);

            // Assert
            postResponse.EnsureSuccessStatusCode();
            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<ReadCategorieDto>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            var id = responseObject!.Id;
            var updateDto = new UpdateCategorieDto
            {
                Name = name,
            };

            jsonContent = new StringContent(
                JsonSerializer.Serialize(updateDto),
                Encoding.UTF8,
                "application/json");

            var response = await _client.PutAsync($"/api/categories/{id}", jsonContent);
            var deleteResponse = await _client.DeleteAsync($"/api/categories/{id}");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
