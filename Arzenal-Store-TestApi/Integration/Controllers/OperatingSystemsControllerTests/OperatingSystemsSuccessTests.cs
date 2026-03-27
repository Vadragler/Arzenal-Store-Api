using Arzenal.Dto.DTOs.OperatingSystemDto;
using System.Net;
using System.Text;
using System.Text.Json;
using Arzenal.Store.Api.TestIntegration.Infrastructure;

namespace Arzenal.Store.Api.TestIntegration.Integration.Controllers.OperatingSystemsControllerTests
{
    public class OperatingSystemsControllerSuccessTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public OperatingSystemsControllerSuccessTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateAuthenticatedClient();
        }

        // ------------------- GET -------------------
        [Fact]
        public async Task GetOperatingSystems_ReturnsList_WhenOsExist()
        {
            // Act
            var response = await _client.GetAsync("api/operatingsystems");

            // Assert
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var osList = JsonSerializer.Deserialize<List<ReadOperatingSystemDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            Assert.NotNull(osList);
            Assert.NotEmpty(osList);
            Assert.Contains(osList, os => os.Name == "Windows");
        }

        [Fact]
        public async Task GetOperatingSystemById_ReturnsOs_WhenIdIsValid()
        {
            // Arrange
            var id = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d");

            // Act
            var response = await _client.GetAsync($"api/operatingsystems/{id}");

            // Assert
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var os = JsonSerializer.Deserialize<ReadOperatingSystemDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.Equal("Windows", os!.Name);
        }

        // ------------------- POST -------------------
        [Fact]
        public async Task PostOperatingSystem_ReturnsCreated_WhenDataIsValid()
        {
            // Arrange
            var newOS = new CreateOperatingSystemDto { Name = "Android" };
            var jsonContent = new StringContent(JsonSerializer.Serialize(newOS), Encoding.UTF8, "application/json");

            // Act
            var postResponse = await _client.PostAsync("api/operatingsystems", jsonContent);

            // Assert
            postResponse.EnsureSuccessStatusCode();

            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var createdOs = JsonSerializer.Deserialize<ReadOperatingSystemDto>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var getResponse = await _client.GetAsync($"api/operatingsystems/{createdOs!.Id}");
            getResponse.EnsureSuccessStatusCode();
            var newOsJson = await getResponse.Content.ReadAsStringAsync();
            var fetchedOs = JsonSerializer.Deserialize<ReadOperatingSystemDto>(newOsJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.Equal("Android", fetchedOs!.Name);
        }

        // ------------------- PUT -------------------
        [Fact]
        public async Task PutOperatingSystem_ReturnsOk_WhenDataIsValid()
        {
            // Arrange
            var newOS = new CreateOperatingSystemDto { Name = "Operating System to Update" };
            var jsonContent = new StringContent(JsonSerializer.Serialize(newOS), Encoding.UTF8, "application/json");

            var postResponse = await _client.PostAsync("api/operatingsystems", jsonContent);
            postResponse.EnsureSuccessStatusCode();
            var createdId = JsonSerializer.Deserialize<JsonElement>(await postResponse.Content.ReadAsStringAsync()).GetProperty("Id");

            // Mise à jour
            var updatedOS = new UpdateOperatingSystemDto { Name = "Operating System Updated" };
            jsonContent = new StringContent(JsonSerializer.Serialize(updatedOS), Encoding.UTF8, "application/json");

            // Act
            var putResponse = await _client.PutAsync($"api/operatingsystems/{createdId}", jsonContent);

            // Assert
            putResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync($"api/operatingsystems/{createdId}");
            getResponse.EnsureSuccessStatusCode();
            var updatedOs = JsonSerializer.Deserialize<ReadOperatingSystemDto>(await getResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.Equal("Operating System Updated", updatedOs!.Name);
        }

        // ------------------- DELETE -------------------
        [Fact]
        public async Task DeleteOperatingSystem_ReturnsNoContent_WhenOsExists()
        {
            // Arrange
            var newOS = new CreateOperatingSystemDto { Name = "OS to Delete" };
            var jsonContent = new StringContent(JsonSerializer.Serialize(newOS), Encoding.UTF8, "application/json");

            var postResponse = await _client.PostAsync("api/operatingsystems", jsonContent);
            postResponse.EnsureSuccessStatusCode();

            var createdId = JsonSerializer.Deserialize<JsonElement>(await postResponse.Content.ReadAsStringAsync()).GetProperty("Id");

            // Act
            var deleteResponse = await _client.DeleteAsync($"api/operatingsystems/{createdId}");

            // Assert
            deleteResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync($"api/operatingsystems/{createdId}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }
    }
}
