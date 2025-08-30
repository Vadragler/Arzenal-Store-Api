using ArzenalStoreSharedDto.DTOs.OperatingSystemDto;
using System.Text;
using System.Text.Json;
using TestArzenalStoreApi.Infrastructure;

namespace TestArzenalStoreApi.Integration.Controllers.OperatingSystemsControllerTests.Tests_Valides
{

    [Collection("SharedDatabaseCollection")]
    public class PutOperatingSystemsSuccessTests
    {

        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;


        public PutOperatingSystemsSuccessTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }
        [Fact]
        public async Task PutOperatingSystem_ReturnsOk_WhenDataIsValid()
        {
            // Arrange
            var newOS = new CreateOperatingSystemDto
            {
                Name = "Operating System to Update",
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

            var updatedOS = new UpdateOperatingSystemDto
            {
                Name = "Operating System Updated"
            };

            jsonContent = new StringContent(
                JsonSerializer.Serialize(updatedOS),
                Encoding.UTF8,
                "application/json");

            // Act
            var putResponse = await _client.PutAsync($"/api/operatingsystems/{id}", jsonContent);

            // Assert
            putResponse.EnsureSuccessStatusCode();

            // Vérifier la mise à jour
            var getResponse = await _client.GetAsync($"/api/operatingsystems/{id}");
            getResponse.EnsureSuccessStatusCode();
            var updatedOsJson = await getResponse.Content.ReadAsStringAsync();
            var updatedOs = JsonSerializer.Deserialize<ReadOperatingSystemDto>(updatedOsJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });


            Assert.Equal("Operating System Updated", updatedOs!.Name);
        }

    }
}
