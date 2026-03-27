using System.Net;
using System.Text;
using Arzenal.Store.Api.TestIntegration.Infrastructure;

namespace Arzenal.Store.Api.TestIntegration.Integration.Controllers.CategoriesControllerTests
{
    public class CategoriesControllerErrorTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public CategoriesControllerErrorTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateAuthenticatedClient();
        }

        // ---------- GET Tests ----------

        [Fact]
        public async Task GetCategorieById_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"api/categories/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetCategorieById_ReturnsBadRequest_WhenIdIsMalformed()
        {
            // Act
            var response = await _client.GetAsync("api/categories/abc");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ---------- DELETE Tests ----------

        [Fact]
        public async Task DeleteCategorie_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"api/categories/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCategorie_ReturnsBadRequest_WhenIdIsMalformed()
        {
            // Act
            var response = await _client.DeleteAsync("api/categories/123456");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ---------- POST Tests ----------

        [Fact]
        public async Task PostCategorie_ReturnUnsupportedMediaType_WhenContentTypeIsInvalid()
        {
            // Arrange
            var invalidContent = new StringContent("{ \"Name\": \"Software\" }", Encoding.UTF8, "text/plain");

            // Act
            var response = await _client.PostAsync("api/categories", invalidContent);

            // Assert
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        }

        // ---------- PUT Tests ----------
        [Fact]
        public async Task PutCategorie_ReturnBadRequest_WhenBodyIsMissing()
        {
            // Arrange
            var id = Guid.NewGuid();
            var jsonContent = new StringContent("", Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"api/categories/{id}", jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

    }
}
