using ArzenalStoreSharedDto.DTOs.TagDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TestArzenalStoreApi.Infrastructure;

namespace TestArzenalStoreApi.Integration.TestsTags.Tests_Valides
{
    [Collection("SharedDatabaseCollection")]
    public class GetTagsSuccessTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;


        public GetTagsSuccessTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }
        [Fact]
        public async Task GetTags_ReturnsList_WhenTagsExist()
        {
            // Act
            var response = await _client.GetAsync("/api/tags");

            // Assert
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var tags = JsonSerializer.Deserialize<List<ReadTagDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(tags);
            Assert.NotEmpty(tags);
            Assert.Contains(tags, l => l.Name == "Productivity");
        }

        [Fact]
        public async Task GetTagsById_ReturnsList_WhenIdIsValid()
        {
            // Act
            var id = 1;
            var response = await _client.GetAsync($"/api/tags/{id}");

            // Assert
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var tags = JsonSerializer.Deserialize<ReadTagDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            
            Assert.Equal("Productivity",tags!.Name);
        }


    }
}
