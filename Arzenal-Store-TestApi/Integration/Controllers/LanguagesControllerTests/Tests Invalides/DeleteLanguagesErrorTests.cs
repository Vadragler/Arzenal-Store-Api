using System.Net;
using TestArzenalStoreApi.Infrastructure;

namespace TestArzenalStoreApi.Integration.Controllers.LanguagesControllerTests.Tests_Invalides
{
    [Collection("SharedDatabaseCollection")]
    public class DeleteLanguagesErrorTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public DeleteLanguagesErrorTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task DeleteLanguage_ReturnsBadRequest_WhenIdIsunder(int invalidId)
        {
          
            // Act
            var response = await _client.DeleteAsync($"/api/languages/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteLanguage_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var invalidId = 999;

            // Act
            var response = await _client.DeleteAsync($"/api/languages/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteLanguage_ReturnsBadRequest_WhenIdIsMalformed()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"/api/languages/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
