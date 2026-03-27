using Arzenal.Dto.DTOs.TagDto;
using System.Net;
using System.Text;
using System.Text.Json;
using Arzenal.Store.Api.TestIntegration.Infrastructure;

namespace Arzenal.Store.Api.TestIntegration.Integration.Controllers.TagsControllerTests
{
    public class TagsControllerErrorTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public TagsControllerErrorTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateAuthenticatedClient();
        }

        // ------------------- GET -------------------
        [Fact]
        public async Task GetTagById_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"api/tags/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetTagById_ReturnsBadRequest_WhenIdIsMalformed()
        {
            // Act
            var response = await _client.GetAsync($"api/tags/abc");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetAllTags_ReturnsOk_WhenNoTagExist()
        {
            // Act
            var response = await _client.GetAsync("api/tags");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ------------------- POST -------------------
        public static IEnumerable<object[]> InvalidNames =>
            new List<object[]>
            {
                new object[] { new string('A', 101) },
                new object[] { "" },
                new object[] { null! },
                new object[] { " " }
            };

        [Theory]
        [MemberData(nameof(InvalidNames))]
        public async Task PostTag_ReturnsBadRequest_WhenDataIsInvalid(string name)
        {
            // Arrange
            var newTag = new CreateTagDto { Name = name };
            var jsonContent = new StringContent(JsonSerializer.Serialize(newTag), Encoding.UTF8, "application/json");

            // Act
            var postResponse = await _client.PostAsync("api/tags", jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, postResponse.StatusCode);
        }

        // ------------------- PUT -------------------
        [Fact]
        public async Task PutTag_ReturnsConflict_WhenTagAlreadyExist()
        {
            // Arrange
            var newTag = new CreateTagDto { Name = "Math" };
            var jsonContent = new StringContent(JsonSerializer.Serialize(newTag), Encoding.UTF8, "application/json");
            var postResponse = await _client.PostAsync("api/tags", jsonContent);
            postResponse.EnsureSuccessStatusCode();

            var createdTag = JsonSerializer.Deserialize<ReadTagDto>(await postResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var updateTag = new UpdateTagDto { Name = "Productivity" };
            jsonContent = new StringContent(JsonSerializer.Serialize(updateTag), Encoding.UTF8, "application/json");

            // Act
            var putResponse = await _client.PutAsync($"api/tags/{createdTag!.Id}", jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, putResponse.StatusCode);
        }

        [Fact]
        public async Task PutTag_ReturnsBadRequest_WhenIdIsMalformed()
        {
            // Arrange
            var updateTag = new UpdateTagDto { Name = "New Name" };
            var jsonContent = new StringContent(JsonSerializer.Serialize(updateTag), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync("api/tags/abc", jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutTag_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var updateTag = new UpdateTagDto { Name = "New Name" };
            var jsonContent = new StringContent(JsonSerializer.Serialize(updateTag), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"api/tags/{Guid.NewGuid()}", jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(InvalidNames))]
        public async Task PutTag_ReturnsBadRequest_WhenFieldIsEmptyOrNull(string name)
        {
            // Arrange
            var newTag = new CreateTagDto { Name = "Temp Tag" };
            var jsonContent = new StringContent(JsonSerializer.Serialize(newTag), Encoding.UTF8, "application/json");
            var postResponse = await _client.PostAsync("api/tags", jsonContent);
            postResponse.EnsureSuccessStatusCode();

            var createdTag = JsonSerializer.Deserialize<ReadTagDto>(await postResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var updateTag = new UpdateTagDto { Name = name };
            jsonContent = new StringContent(JsonSerializer.Serialize(updateTag), Encoding.UTF8, "application/json");

            // Act
            var putResponse = await _client.PutAsync($"api/tags/{createdTag!.Id}", jsonContent);

            // Nettoyage
            
            var deleteResponse = await _client.DeleteAsync($"api/tags/{createdTag.Id}");
            deleteResponse.EnsureSuccessStatusCode();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, putResponse.StatusCode);
        }

        // ------------------- DELETE -------------------
        [Fact]
        public async Task DeleteTag_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Act
            var response = await _client.DeleteAsync($"api/tags/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteTag_ReturnsBadRequest_WhenIdIsMalformed()
        {
            // Act
            var response = await _client.DeleteAsync("api/tags/123456");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
