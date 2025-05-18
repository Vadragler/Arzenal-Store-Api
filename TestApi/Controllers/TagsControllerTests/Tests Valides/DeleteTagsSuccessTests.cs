using ArzenalApi.Data;
using ArzenalApi.DTOs.AppDto;
using ArzenalApi.DTOs.LanguageDto;
using ArzenalApi.DTOs.OperatingSystemDto;
using ArzenalApi.DTOs.TagDto;
using ArzenalApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TestApi.Infrastructure;

namespace TestApi.TestsTags.Tests_Valides
{
    [Collection("SharedDatabaseCollection")]
    public class DeleteTagsSuccessTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;


        public DeleteTagsSuccessTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }


        [Fact]
        public async Task DeleteTag_ReturnsNoContent_WhenTagExists()
        {
            // Arrange
            var newTag = new CreateTagDto
            {
                Name = "Tags to Delete",
            };
            // Créer l'application via POST pour avoir un élément à supprimer
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(newTag),
                Encoding.UTF8,
                "application/json");

            var postResponse = await _client.PostAsync("/api/tags", jsonContent);
            postResponse.EnsureSuccessStatusCode();
            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseJson);
            var id = responseObject.GetProperty("id");
            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/tags/{id}");

            // Assert
            deleteResponse.EnsureSuccessStatusCode();

            // Vérifier si l'élément a été supprimé
            var getResponse = await _client.GetAsync($"/api/tags/{id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteLanguages()
        {
            // Arrange


            var getResponse = await _client.GetAsync("/api/apps");
            getResponse.EnsureSuccessStatusCode();
            var responseJson = await getResponse.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<List<ReadAppDto>>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Supprimer tous les tags existants
            foreach (var tag in responseObject)
            {
                var id = tag.Id; // Récupérer l'ID du tag
                var deleteResponse = await _client.DeleteAsync($"/api/apps/{id}");

                // Vérifier que la suppression a réussi
                deleteResponse.EnsureSuccessStatusCode();
            }

            // Vérifier si tous les tags ont bien été supprimés
            foreach (var tag in responseObject)
            {
                var id = tag.Id;
                var getResponseAfterDelete = await _client.GetAsync($"/api/apps/{id}");
                Assert.Equal(HttpStatusCode.NotFound, getResponseAfterDelete.StatusCode); // Vérifier que chaque tag est supprimé
            }
        }

    }
}
