using Arzenal.Dto.DTOs.OperatingSystemDto;
using Arzenal.Store.Api.TestIntegration.Infrastructure;
using System.Net;
using System.Text;
using System.Text.Json;

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
            var userId = Guid.Parse("d3f1a9e2-5b6c-4f7d-9a12-8c3e4b5f6789");
            var deviceName = "IntegrationTestDevice";

            // 1️⃣ Serialize le DTO une seule fois
            var jsonBody = JsonSerializer.Serialize(newOs);

            // 2️⃣ Calculer signature
            var url = "api/operatingsystems/";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ------------------- PUT -------------------
        [Fact]
        public async Task PutOperatingSystem_ReturnsConflict_WhenOperatingSystemAlreadyExist()
        {
            // Arrange
            var newOs = new CreateOperatingSystemDto { Name = "IOS" };
            var userId = Guid.Parse("d3f1a9e2-5b6c-4f7d-9a12-8c3e4b5f6789");
            var deviceName = "IntegrationTestDevice";

            // 1️⃣ Serialize le DTO une seule fois
            var jsonBody = JsonSerializer.Serialize(newOs);

            // 2️⃣ Calculer signature
            var url = "api/operatingsystems/";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var postResponse = await _client.SendAsync(request);
            postResponse.EnsureSuccessStatusCode();

            var createdOs = JsonSerializer.Deserialize<ReadOperatingSystemDto>(await postResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(createdOs);
            var updateOs = new UpdateOperatingSystemDto { Name = "Windows" };

            // 1️⃣ Serialize le DTO une seule fois
            jsonBody = JsonSerializer.Serialize(updateOs);

            // 2️⃣ Calculer signature
            url = $"api/operatingsystems/{createdOs.Id}";

            request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var putResponse = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, putResponse.StatusCode);
        }

        [Fact]
        public async Task PutOperatingSystem_ReturnsBadRequest_WhenIdIsMalformed()
        {
            // Arrange
            var updateDto = new UpdateOperatingSystemDto { Name = "New Name" };
            var userId = Guid.Parse("d3f1a9e2-5b6c-4f7d-9a12-8c3e4b5f6789");
            var deviceName = "IntegrationTestDevice";

            // 1️⃣ Serialize le DTO une seule fois
            var jsonBody = JsonSerializer.Serialize(updateDto);

            // 2️⃣ Calculer signature
            var url = "api/operatingsystems/abc";

            var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutOperatingSystem_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var updateDto = new UpdateOperatingSystemDto { Name = "New Name" };
            var userId = Guid.Parse("d3f1a9e2-5b6c-4f7d-9a12-8c3e4b5f6789");
            var deviceName = "IntegrationTestDevice";

            // 1️⃣ Serialize le DTO une seule fois
            var jsonBody = JsonSerializer.Serialize(updateDto);

            // 2️⃣ Calculer signature
            var url = $"api/operatingsystems/{Guid.NewGuid()}";

            var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(InvalidNames))]
        public async Task PutOperatingSystem_ReturnsBadRequest_WhenFieldIsEmptyOrNull(string name)
        {
            // Arrange
            var newOs = new CreateOperatingSystemDto { Name = "Nom valide" };
            var userId = Guid.Parse("d3f1a9e2-5b6c-4f7d-9a12-8c3e4b5f6789");
            var deviceName = "IntegrationTestDevice";

            // 1️⃣ Serialize le DTO une seule fois
            var jsonBody = JsonSerializer.Serialize(newOs);

            // 2️⃣ Calculer signature
            var url = "api/operatingsystems/";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var postResponse = await _client.SendAsync(request);
            postResponse.EnsureSuccessStatusCode();

            var createdOs = JsonSerializer.Deserialize<ReadOperatingSystemDto>(await postResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(createdOs);

            // Tenter de mettre à jour avec un nom invalide
            var updateDto = new UpdateOperatingSystemDto { Name = name };

            // 1️⃣ Serialize le DTO une seule fois
            jsonBody = JsonSerializer.Serialize(updateDto);

            // 2️⃣ Calculer signature
            url = $"api/operatingsystems/{createdOs.Id}";

            request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var putResponse = await _client.SendAsync(request);

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
