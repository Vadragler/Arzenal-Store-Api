using ArzenalStoreSharedDto.DTOs.TagDto;
using ArzenalStoreSharedDto.DTOs.OperatingSystemDto;
using System.Net;
using System.Text;
using System.Text.Json;
using TestArzenalStoreApi.Infrastructure;

namespace TestArzenalStoreApi.Integration.TestsTags.Tests_Invalides
{
    [Collection("SharedDatabaseCollection")]
    public class PutTagsErrorTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;


        public PutTagsErrorTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task PutTag_ReturnConflict_WhenTagAlreadyExist()
        {
            // Arrange
            var newTag = new CreateOperatingSystemDto { Name = "Math" };
            var jsonContent = new StringContent(
               JsonSerializer.Serialize(newTag),
               Encoding.UTF8,
               "application/json");
            // Act
            var postResponse = await _client.PostAsync("/api/tags", jsonContent);
            postResponse.EnsureSuccessStatusCode();
            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<ReadOperatingSystemDto>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            var id = responseObject!.Id;
            var updatedTag = new UpdateOperatingSystemDto { Name = "Productivity" };
            jsonContent = new StringContent(
                JsonSerializer.Serialize(updatedTag),
                Encoding.UTF8,
                "application/json");
            var putResponse = await _client.PutAsync($"/api/tags/{id}", jsonContent);
            // Assert
            Assert.Equal(HttpStatusCode.Conflict, putResponse.StatusCode);
        }

        [Fact]
        public async Task PutTag_ReturnBadRequest_WhenIdIsMalformed()
        {
            var updateDto = new UpdateTagDto
            {
                Name = "New Name",
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(updateDto),
                Encoding.UTF8,
                "application/json");
            var abc = "abc";
            var response = await _client.PutAsync($"/api/tags/{abc}", jsonContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task PutTag_ReturnBadRequest_WhenIdIsUnder(int id)
        {
            var updateDto = new UpdateTagDto
            {
                Name = "New Name",
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(updateDto),
                Encoding.UTF8,
                "application/json");
            var response = await _client.PutAsync($"/api/tags/{id}", jsonContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutTag_ReturnNotFound_WhenIdDoesNotExist()
        {
            var updateDto = new UpdateTagDto
            {
                Name = "New Name",
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(updateDto),
                Encoding.UTF8,
                "application/json");
            var id = 999;
            var response = await _client.PutAsync($"/api/tags/{id}", jsonContent);

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
        public async Task PutTag_ReturnBadRequest_WhenFieldIsEmptyorNull(string name)
        {
            var newTag = new CreateTagDto
            {
                Name = "Nom vide"
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(newTag),
                Encoding.UTF8,
                "application/json");

            // Act
            var postResponse = await _client.PostAsync("/api/tags", jsonContent);

            // Assert
            postResponse.EnsureSuccessStatusCode();
            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<ReadTagDto>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            var id = responseObject!.Id;
            var updateDto = new UpdateTagDto
            {
                Name = name,
            };

            jsonContent = new StringContent(
                JsonSerializer.Serialize(updateDto),
                Encoding.UTF8,
                "application/json");
            
            var response = await _client.PutAsync($"/api/tags/{id}", jsonContent);
            var deleteResponse = await _client.DeleteAsync($"/api/tags/{id}");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

    }
}
