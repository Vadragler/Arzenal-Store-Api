using ArzenalApi.Data;
using Arzenal.Shared.Dtos.DTOs.TagDto;
using ArzenalApi.Models;
using Microsoft.Extensions.DependencyInjection;
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
    public class PutTagsSuccessTests
    {

        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;


        public PutTagsSuccessTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }


        [Fact]
        public async Task PutTag_ReturnsOk_WhenDataIsValid()
        {
            // Arrange
            var newTag = new CreateTagDto
            {
                Name = "Tags to Update",
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

            var updatedTag = new UpdateTagDto
            {
                Name = "Tag Updated"
            };

            jsonContent = new StringContent(
                JsonSerializer.Serialize(updatedTag),
                Encoding.UTF8,
                "application/json");

            // Act
            var putResponse = await _client.PutAsync($"/api/tags/{id}", jsonContent);

            // Assert
            putResponse.EnsureSuccessStatusCode();

            // Vérifier la mise à jour
            var getResponse = await _client.GetAsync($"/api/tags/{id}");
            getResponse.EnsureSuccessStatusCode();
            var updatedTagJson = await getResponse.Content.ReadAsStringAsync();
            var updatedTa = JsonSerializer.Deserialize<ReadTagDto>(updatedTagJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });


            Assert.Equal("Tag Updated", updatedTa!.Name);
        }

    }
}
