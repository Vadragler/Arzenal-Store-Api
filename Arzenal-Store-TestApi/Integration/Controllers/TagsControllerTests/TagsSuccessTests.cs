using Arzenal.Dto.DTOs.TagDto;
using System.Net;
using System.Text;
using System.Text.Json;
using Arzenal.Store.Api.TestIntegration.Infrastructure;

namespace Arzenal.Store.Api.TestIntegration.Integration.Controllers.TagsControllerTests
{
    public class TagsControllerSuccessTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public TagsControllerSuccessTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateAuthenticatedClient();
        }

        // ------------------- GET -------------------
        [Fact]
        public async Task GetTags_ReturnsList_WhenTagsExist()
        {
            // Act
            var response = await _client.GetAsync("api/tags");

            // Assert
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var tags = JsonSerializer.Deserialize<List<ReadTagDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(tags);
            Assert.NotEmpty(tags);
            Assert.Contains(tags, t => t.Name == "Productivity");
        }

        [Fact]
        public async Task GetTagById_ReturnsTag_WhenIdIsValid()
        {
            // Arrange
            var id = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d");

            // Act
            var response = await _client.GetAsync($"api/tags/{id}");

            // Assert
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var tag = JsonSerializer.Deserialize<ReadTagDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.Equal("Productivity", tag!.Name);
        }

        // ------------------- POST -------------------
        [Fact]
        public async Task PostTag_ReturnsCreated_WhenDataIsValid()
        {
            // Arrange
            var newTag = new CreateTagDto { Name = "Services" };
            var jsonContent = new StringContent(JsonSerializer.Serialize(newTag), Encoding.UTF8, "application/json");

            // Act
            var postResponse = await _client.PostAsync("api/tags", jsonContent);

            // Assert
            postResponse.EnsureSuccessStatusCode();

            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var createdTag = JsonSerializer.Deserialize<ReadTagDto>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var getResponse = await _client.GetAsync($"api/tags/{createdTag!.Id}");
            getResponse.EnsureSuccessStatusCode();

            var newTagJson = await getResponse.Content.ReadAsStringAsync();
            var fetchedTag = JsonSerializer.Deserialize<ReadTagDto>(newTagJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            Assert.Equal("Services", fetchedTag!.Name);
        }

        // ------------------- PUT -------------------
        [Fact]
        public async Task PutTag_ReturnsOk_WhenDataIsValid()
        {
            // Arrange
            var newTag = new CreateTagDto { Name = "Tags to Update" };
            var jsonContent = new StringContent(JsonSerializer.Serialize(newTag), Encoding.UTF8, "application/json");

            var postResponse = await _client.PostAsync("api/tags", jsonContent);
            postResponse.EnsureSuccessStatusCode();

            var responseObject = JsonSerializer.Deserialize<JsonElement>(await postResponse.Content.ReadAsStringAsync());
            var id = responseObject.GetProperty("Id");

            // Mise à jour
            var updatedTag = new UpdateTagDto { Name = "Tag Updated" };
            jsonContent = new StringContent(JsonSerializer.Serialize(updatedTag), Encoding.UTF8, "application/json");

            // Act
            var putResponse = await _client.PutAsync($"api/tags/{id}", jsonContent);

            // Assert
            putResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync($"api/tags/{id}");
            getResponse.EnsureSuccessStatusCode();

            var updatetTag = JsonSerializer.Deserialize<ReadTagDto>(await getResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            Assert.Equal("Tag Updated", updatetTag!.Name);
        }

        // ------------------- DELETE -------------------
        [Fact]
        public async Task DeleteTag_ReturnsNoContent_WhenTagExists()
        {
            // Arrange
            var newTag = new CreateTagDto { Name = "Tag to Delete" };
            var jsonContent = new StringContent(JsonSerializer.Serialize(newTag), Encoding.UTF8, "application/json");

            var postResponse = await _client.PostAsync("api/tags", jsonContent);
            postResponse.EnsureSuccessStatusCode();

            var responseObject = JsonSerializer.Deserialize<JsonElement>(await postResponse.Content.ReadAsStringAsync());
            var id = responseObject.GetProperty("Id").GetGuid();

            // Act
            var deleteResponse = await _client.DeleteAsync($"api/tags/{id}");

            // Assert
            deleteResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync($"api/tags/{id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }
    }
}
