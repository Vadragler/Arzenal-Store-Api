using ArzenalStoreSharedDto.DTOs.AppDto;
using System.Net;
using System.Text.Json;
using TestArzenalStoreApi.Infrastructure;

namespace TestArzenalStoreApi.Integration.Controllers.AppsControllerTests.Tests_Invalides
{
    [Collection("SharedDatabaseCollection")]
    public class GetAppErrorTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;


        public GetAppErrorTests(CustomWebApplicationFactory<Program> factory)
        {
             _factory = factory;
            _client = _factory.CreateClient();
        }

       

        [Fact]
        public async Task GetById_ReturnNotFound_WhenIdIsInvalid()
        {
            // Arrange
            var invalidId = Guid.NewGuid(); // Nouvelle Id qui n'existe pas dans la base
            var response = await _client.GetAsync($"/api/apps/{invalidId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public  async Task GetById_ReturnBadrequest_WhenIdIsMalformed()
        {
            // Arrange
            var MalFormedId = "abc";
            var response = await _client.GetAsync($"/api/apps/{MalFormedId}");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetById_ReturnBadrequest_WhenIdIsint()
        {
            // Arrange
            var MalFormedId = 1;
            var response = await _client.GetAsync($"/api/apps/{MalFormedId}");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetAllApps_ReturnsEmptyList_WhenNoAppsExist()
        {
            // Arrange
            var (_, client) = TestHelpers.CreateClientWithEmptyDb<Program>();

            // Act
            var response = await client.GetAsync("/api/apps");

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var contentString = await response.Content.ReadAsStringAsync();
            var content = JsonSerializer.Deserialize<JsonElement>(contentString);
            var expected = new { error = "Aucune application trouvée." };
            Assert.Equal(JsonSerializer.Serialize(expected), JsonSerializer.Serialize(content));
        }

    }
}
