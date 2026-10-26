using Arzenal.Dto.DTOs.TagDto;
using Arzenal.Store.Api.TestIntegration.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

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
            var userId = Guid.Parse("d3f1a9e2-5b6c-4f7d-9a12-8c3e4b5f6789");
            var deviceName = "IntegrationTestDevice";

            // 1️⃣ Serialize le DTO une seule fois
            var jsonBody = JsonSerializer.Serialize(newTag);

            // 2️⃣ Calculer signature
            var url = "api/tags/";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var postResponse = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, postResponse.StatusCode);
        }

        // ------------------- PUT -------------------
        [Fact]
        public async Task PutTag_ReturnsConflict_WhenTagAlreadyExist()
        {
            // Arrange
            var newTag = new CreateTagDto { Name = "Math" };
            var userId = Guid.Parse("d3f1a9e2-5b6c-4f7d-9a12-8c3e4b5f6789");
            var deviceName = "IntegrationTestDevice";

            // 1️⃣ Serialize le DTO une seule fois
            var jsonBody = JsonSerializer.Serialize(newTag);

            // 2️⃣ Calculer signature
            var url = $"api/tags/";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var postResponse = await _client.SendAsync(request);
            postResponse.EnsureSuccessStatusCode();

            var createdTag = JsonSerializer.Deserialize<ReadTagDto>(await postResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var updateTag = new UpdateTagDto { Name = "Productivity" };

            // 1️⃣ Serialize le DTO une seule fois
            jsonBody = JsonSerializer.Serialize(updateTag);

            // 2️⃣ Calculer signature
            url = $"api/tags/{createdTag!.Id}";

            request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var putResponse = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, putResponse.StatusCode);
        }

        [Fact]
        public async Task PutTag_ReturnsBadRequest_WhenIdIsMalformed()
        {
            // Arrange
            var updateTag = new UpdateTagDto { Name = "New Name" };
            var userId = Guid.Parse("d3f1a9e2-5b6c-4f7d-9a12-8c3e4b5f6789");
            var deviceName = "IntegrationTestDevice";

            // 1️⃣ Serialize le DTO une seule fois
            var jsonBody = JsonSerializer.Serialize(updateTag);

            // 2️⃣ Calculer signature
            var url = "api/tags/abc";

            var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutTag_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var updateTag = new UpdateTagDto { Name = "New Name" };
            var userId = Guid.Parse("d3f1a9e2-5b6c-4f7d-9a12-8c3e4b5f6789");
            var deviceName = "IntegrationTestDevice";

            // 1️⃣ Serialize le DTO une seule fois
            var jsonBody = JsonSerializer.Serialize(updateTag);

            // 2️⃣ Calculer signature
            var url = $"api/tags/{Guid.NewGuid()}";

            var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(InvalidNames))]
        public async Task PutTag_ReturnsBadRequest_WhenFieldIsEmptyOrNull(string name)
        {
            // Arrange
            var newTag = new CreateTagDto { Name = "Temp Tag" };
            var userId = Guid.Parse("d3f1a9e2-5b6c-4f7d-9a12-8c3e4b5f6789");
            var deviceName = "IntegrationTestDevice";

            // 1️⃣ Serialize le DTO une seule fois
            var jsonBody = JsonSerializer.Serialize(newTag);

            // 2️⃣ Calculer signature
            var url = $"api/tags/";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var postResponse = await _client.SendAsync(request);
            postResponse.EnsureSuccessStatusCode();

            var createdTag = JsonSerializer.Deserialize<ReadTagDto>(await postResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(createdTag);

            var updateTag = new UpdateTagDto { Name = name };

            // 1️⃣ Serialize le DTO une seule fois
            jsonBody = JsonSerializer.Serialize(updateTag);

            // 2️⃣ Calculer signature
            url = $"api/tags/{createdTag.Id}";

            request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var putResponse = await _client.SendAsync(request);

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
