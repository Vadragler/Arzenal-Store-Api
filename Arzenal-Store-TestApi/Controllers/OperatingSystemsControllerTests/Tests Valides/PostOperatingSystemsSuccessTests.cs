using Arzenal.Shared.Dtos.DTOs.OperatingSystemDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TestApi.Infrastructure;

namespace TestApi.Controllers.OperatingSystemsControllerTests.Tests_Valides
{
    [Collection("SharedDatabaseCollection")]
    public class PostOperatingSystemsSuccessTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;


        public PostOperatingSystemsSuccessTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task PostOperatingSystem_ReturnsCreated_WhenDataIsValid()
        {
            // Arrange
            var newOS = new CreateOperatingSystemDto
            {
                Name = "Android"
            };
            // Créer l'application via POST pour avoir un élément à supprimer
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(newOS),
                Encoding.UTF8,
                "application/json");
            // Act
            var postResponse = await _client.PostAsync("/api/operatingsystems", jsonContent);
            postResponse.EnsureSuccessStatusCode();
            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<ReadOperatingSystemDto>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            var id = responseObject!.Id;
            

            // Assert
            
            // Vérifier que l'élément est bien ajouté
            var getResponse = await _client.GetAsync($"/api/operatingsystems/{id}");
            getResponse.EnsureSuccessStatusCode();
            var newOsJson = await getResponse.Content.ReadAsStringAsync();
            var createdOs = JsonSerializer.Deserialize<ReadOperatingSystemDto>(newOsJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.Equal("Android", createdOs!.Name);
        }

    }
}
