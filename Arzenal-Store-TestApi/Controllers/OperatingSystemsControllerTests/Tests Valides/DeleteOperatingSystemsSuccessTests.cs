using ArzenalApi.Data;
using Arzenal.Shared.Dtos.DTOs.AppDto;
using Arzenal.Shared.Dtos.DTOs.OperatingSystemDto;
using ArzenalApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TestApi.Infrastructure;

namespace TestApi.Controllers.OperatingSystemsControllerTests.Tests_Valides
{
    [Collection("SharedDatabaseCollection")]
    public class DeleteOperatingSystemsSuccessTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;


        public DeleteOperatingSystemsSuccessTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task DeleteOperatingSystem_ReturnsNoContent_WhenTagExists()
        {
            // Arrange
            var newOS = new CreateOperatingSystemDto
            {
                Name = "OS to Delete",
            };
            // Créer l'application via POST pour avoir un élément à supprimer
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(newOS),
                Encoding.UTF8,
                "application/json");

            var postResponse = await _client.PostAsync("/api/operatingsystems", jsonContent);
            postResponse.EnsureSuccessStatusCode();
            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseJson);
            var id = responseObject.GetProperty("id");
            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/operatingsystems/{id}");

            // Assert
            deleteResponse.EnsureSuccessStatusCode();

            // Vérifier si l'élément a été supprimé
            var getResponse = await _client.GetAsync($"/api/operatingsystems/{id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

    }
}
