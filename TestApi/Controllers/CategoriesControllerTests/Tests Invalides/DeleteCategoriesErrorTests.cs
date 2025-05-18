using System.Net;
using TestApi.Infrastructure;

namespace TestApi.Controllers.CategoriesControllerTests.Tests_Invalides
{
    [Collection("SharedDatabaseCollection")]
    public class DeleteCategoriesErrorTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public DeleteCategoriesErrorTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task DeleteCategorie_ReturnsBadRequest_WhenIdIsunder(int invalidId)
        {
            // Arrange


            // Act
            var response = await _client.DeleteAsync($"/api/categories/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCategorie_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var invalidId = 999;

            // Act
            var response = await _client.DeleteAsync($"/api/categories/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCategorie_ReturnsBadRequest_WhenIdIsMalformed()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"/api/categories/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

    }
}
