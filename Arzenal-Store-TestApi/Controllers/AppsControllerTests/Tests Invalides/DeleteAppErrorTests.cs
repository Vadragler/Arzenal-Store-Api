using System.Net;
using TestApi.Infrastructure;

namespace TestApi.Controllers.AppsControllerTests.Tests_Invalides
{
    [Collection("SharedDatabaseCollection")]
    public class DeleteAppErrorTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public DeleteAppErrorTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task DeleteApp_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"/api/apps/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteApp_ReturnsBadRequest_WhenIdIsInt()
        {
            // Arrange
            var invalidId = 1;

            // Act
            var response = await _client.DeleteAsync($"/api/apps/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteApp_ReturnsBadRequest_WhenIdIsMalformed()
        {
            // Arrange
            var invalidId = "abc";

            // Act
            var response = await _client.DeleteAsync($"/api/apps/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


    }
}
