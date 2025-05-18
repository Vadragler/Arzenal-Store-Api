using Arzenal.Shared.Dtos.DTOs.LanguageDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TestApi.Infrastructure;

namespace TestApi.Controllers.OperatingSystemsControllerTests.Tests_Invalides
{
    [Collection("SharedDatabaseCollection")]
    public class GetOperatingSystemsErrorTests
    {

        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;


        public GetOperatingSystemsErrorTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task GetOperatingSystemById_ReturnsBadRequest_WhenIdIsunder(int invalidId)
        {
            // Arrange


            // Act
            var response = await _client.GetAsync($"/api/operatingSystems/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetOperatingSystemById_ReturnNotFound_WhenIdIsInvalid()
        {
            // Arrange
            var invalidId = 999; // Nouvelle Id qui n'existe pas dans la base
            var response = await _client.GetAsync($"/api/operatingsystems/{invalidId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetOperatingSystemById_ReturnBadrequest_WhenIdIsMalformed()
        {
            // Arrange
            var MalFormedId = "abc";
            var response = await _client.GetAsync($"/api/operatingsystems/{MalFormedId}");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetAllOperatingSystem_ReturnsEmptyList_WhenNoLanguageExist()
        {
            // Arrange
            var (_, client2) = TestHelpers.CreateClientWithEmptyDb<Program>();
            var response = await client2.GetAsync("/api/operatingsystems");

            //Act
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var operatingsystems = JsonSerializer.Deserialize<List<ReadLanguageDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            //Assert
            Assert.NotNull(operatingsystems);
            Assert.Empty(operatingsystems);
        }
    }
}
