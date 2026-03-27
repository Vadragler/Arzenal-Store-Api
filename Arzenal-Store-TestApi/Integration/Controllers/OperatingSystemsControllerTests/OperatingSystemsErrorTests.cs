using Arzenal.Dto.DTOs.OperatingSystemDto;
using System.Net;
using System.Text;
using System.Text.Json;
using Arzenal.Store.Api.TestIntegration.Infrastructure;

namespace Arzenal.Store.Api.TestIntegration.Integration.Controllers.OperatingSystemsControllerTests
{
    public class OperatingSystemsControllerErrorTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public OperatingSystemsControllerErrorTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateAuthenticatedClient();
        }

        // ------------------- GET -------------------
        [Fact]
        public async Task GetOperatingSystemById_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"api/operatingsystems/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetOperatingSystemById_ReturnsBadRequest_WhenIdIsMalformed()
        {
            // Arrange
            var malformedId = "abc";

            // Act
            var response = await _client.GetAsync($"api/operatingsystems/{malformedId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetAllOperatingSystems_ReturnsOk_WhenNoOperatingSystemsExist()
        {
            // Act
            var response = await _client.GetAsync("api/operatingsystems");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ------------------- POST -------------------
        public static IEnumerable<object[]> InvalidNames =>
            new List<object[]>
            {
                new object[] { new string('A', 101) },
                new object[] { "" },
                new object[] { null! },
                new object[] { " " }
            };

        [Theory]
        [MemberData(nameof(InvalidNames))]
        public async Task PostOperatingSystem_ReturnsBadRequest_WhenDataIsInvalid(string name)
        {
            // Arrange
            var newOs = new CreateOperatingSystemDto { Name = name };
            var jsonContent = new StringContent(JsonSerializer.Serialize(newOs), Encoding.UTF8, "application/json");
            
            // Act
            var response = await _client.PostAsync("api/operatingsystems", jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ------------------- PUT -------------------
        [Fact]
        public async Task PutOperatingSystem_ReturnsConflict_WhenOperatingSystemAlreadyExist()
        {
            // Arrange
            var newOs = new CreateOperatingSystemDto { Name = "IOS" };
            var jsonContent = new StringContent(JsonSerializer.Serialize(newOs), Encoding.UTF8, "application/json");

            var postResponse = await _client.PostAsync("api/operatingsystems", jsonContent);
            postResponse.EnsureSuccessStatusCode();

            var createdOs = JsonSerializer.Deserialize<ReadOperatingSystemDto>(await postResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var updateOs = new UpdateOperatingSystemDto { Name = "Windows" };
            jsonContent = new StringContent(JsonSerializer.Serialize(updateOs), Encoding.UTF8, "application/json");

            // Act
            var putResponse = await _client.PutAsync($"api/operatingsystems/{createdOs!.Id}", jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, putResponse.StatusCode);
        }

        [Fact]
        public async Task PutOperatingSystem_ReturnsBadRequest_WhenIdIsMalformed()
        {
            // Arrange
            var updateDto = new UpdateOperatingSystemDto { Name = "New Name" };
            var jsonContent = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"api/operatingsystems/abc-h", jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutOperatingSystem_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var updateDto = new UpdateOperatingSystemDto { Name = "New Name" };
            var jsonContent = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"api/operatingsystems/{Guid.NewGuid()}", jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(InvalidNames))]
        public async Task PutOperatingSystem_ReturnsBadRequest_WhenFieldIsEmptyOrNull(string name)
        {
            // Arrange
            var newOs = new CreateOperatingSystemDto { Name = "Nom valide" };
            var jsonContent = new StringContent(JsonSerializer.Serialize(newOs), Encoding.UTF8, "application/json");
            var postResponse = await _client.PostAsync("api/operatingsystems", jsonContent);
            postResponse.EnsureSuccessStatusCode();

            var createdOs = JsonSerializer.Deserialize<ReadOperatingSystemDto>(await postResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Tenter de mettre à jour avec un nom invalide
            var updateDto = new UpdateOperatingSystemDto { Name = name };
            jsonContent = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");

            // Act
            var putResponse = await _client.PutAsync($"api/operatingsystems/{createdOs!.Id}", jsonContent);

            // Nettoyer
            var deleteResponse = await _client.DeleteAsync($"api/operatingsystems/{createdOs.Id}");
            deleteResponse.EnsureSuccessStatusCode();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, putResponse.StatusCode);
        }

        // ------------------- DELETE -------------------
        [Fact]
        public async Task DeleteOperatingSystem_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Act
            var response = await _client.DeleteAsync($"api/operatingsystems/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteOperatingSystem_ReturnsBadRequest_WhenIdIsMalformed()
        {
            // Act
            var response = await _client.DeleteAsync($"api/operatingsystems/123456");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
