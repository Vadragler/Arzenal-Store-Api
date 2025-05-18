using Arzenal.Shared.Dtos.DTOs.TagDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TestApi.Infrastructure;

namespace TestApi.TestsTags.Tests_Invalides
{
    [Collection("SharedDatabaseCollection")]
    public class GetTagsErrorTests
    {

        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;


        public GetTagsErrorTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task GetTagById_ReturnsBadRequest_WhenIdIsunder(int invalidId)
        {
            // Arrange


            // Act
            var response = await _client.GetAsync($"/api/tags/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetTagById_ReturnNotFound_WhenIdIsInvalid()
        {
            // Arrange
            var invalidId = 999; // Nouvelle Id qui n'existe pas dans la base
            var response = await _client.GetAsync($"/api/tags/{invalidId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetTagById_ReturnBadrequest_WhenIdIsMalformed()
        {
            // Arrange
            var MalFormedId = "abc";
            var response = await _client.GetAsync($"/api/tags/{MalFormedId}");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetAllTag_ReturnsEmptyList_WhenNoLanguageExist()
        {
            // Arrange
            var (_, client2) = TestHelpers.CreateClientWithEmptyDb<Program>();
            var response = await client2.GetAsync("/api/tags");

            //Act
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var tag = JsonSerializer.Deserialize<List<ReadTagDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            //Assert
            Assert.NotNull(tag);
            Assert.Empty(tag);
        }
    }
}
