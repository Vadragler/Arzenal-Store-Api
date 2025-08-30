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
    public class PostTagsSuccessTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;


        public PostTagsSuccessTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task PostTags_ReturnsCreated_WhenDataIsValid()
        {
            // Arrange
            var newTag = new CreateTagDto
            {
                Name = "Services"
            };
            // Créer l'application via POST pour avoir un élément à supprimer
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(newTag),
                Encoding.UTF8,
                "application/json");
            // Act
            var postResponse = await _client.PostAsync("/api/tags", jsonContent);
            postResponse.EnsureSuccessStatusCode();
            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<ReadTagDto>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            var id = responseObject!.Id;

            

            // Assert
            
            // Vérifier que l'élément est bien ajouté
            var getResponse = await _client.GetAsync($"/api/tags/{id}");
            getResponse.EnsureSuccessStatusCode();
            var newTagJson = await getResponse.Content.ReadAsStringAsync();
            var createdTag = JsonSerializer.Deserialize<ReadTagDto>(newTagJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            var deleteResponse = await _client.DeleteAsync($"/api/tags/{id}");

            // Assert
            deleteResponse.EnsureSuccessStatusCode();

            Assert.Equal("Services", createdTag!.Name);
        }



    }
}
