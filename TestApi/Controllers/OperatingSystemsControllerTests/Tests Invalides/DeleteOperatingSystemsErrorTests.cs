using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TestApi.Infrastructure;

namespace TestApi.Controllers.OperatingSystemsControllerTests.Tests_Invalides
{
    [Collection("SharedDatabaseCollection")]
    public class DeleteOperatingSystemsErrorTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public DeleteOperatingSystemsErrorTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task DeleteOperatingSystem_ReturnsBadRequest_WhenIdIsunder(int invalidId)
        {
            // Arrange
           

            // Act
            var response = await _client.DeleteAsync($"/api/operatingSystems/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        [Fact]
        public async Task DeleteOperatingSystem_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var invalidId = 999;

            // Act
            var response = await _client.DeleteAsync($"/api/operatingSystems/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteOperatingSystem_ReturnsBadRequest_WhenIdIsMalformed()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"/api/operatingsystems/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
