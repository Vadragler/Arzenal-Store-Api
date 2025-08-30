using ArzenalStoreSharedDto.DTOs.OperatingSystemDto;
using System.Net;
using System.Text;
using System.Text.Json;
using TestArzenalStoreApi.Infrastructure;

namespace TestArzenalStoreApi.Integration.Controllers.OperatingSystemsControllerTests.Tests_Invalides
{
    [Collection("SharedDatabaseCollection")]
    public class PutOperatingSystemsErrorTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;


        public PutOperatingSystemsErrorTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task PutOperatingSystem_Conflict_WhenOperatingSystemAlreadyExist()
        {
            // Arrange
            var newOperatingSystem = new CreateOperatingSystemDto { Name = "IOS" };
            var jsonContent = new StringContent(
               JsonSerializer.Serialize(newOperatingSystem),
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
            var updatedOperatingSystem = new UpdateOperatingSystemDto { Name = "Windows" };
            jsonContent = new StringContent(
                JsonSerializer.Serialize(updatedOperatingSystem),
                Encoding.UTF8,
                "application/json");
            var putResponse = await _client.PutAsync($"/api/operatingsystems/{id}", jsonContent);
            // Assert
            Assert.Equal(HttpStatusCode.Conflict, putResponse.StatusCode);
        }

        [Fact]
        public async Task PutOperatingSystem_ReturnBadRequest_WhenIdIsMalformed()
        {
            var updateDto = new UpdateOperatingSystemDto
            {
                Name = "New Name",
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(updateDto),
                Encoding.UTF8,
                "application/json");
            var abc = "abc-h";
            var response = await _client.PutAsync($"/api/operatingsystems/{abc}", jsonContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task PutOperatingSystem_ReturnBadRequest_WhenIdIsUnder(int id)
        {
            var updateDto = new UpdateOperatingSystemDto
            {
                Name = "New Name",
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(updateDto),
                Encoding.UTF8,
                "application/json");
            var response = await _client.PutAsync($"/api/operatingsystems/{id}", jsonContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutOperatingSystem_ReturnNotFound_WhenIdDoesNotExist()
        {
            var updateDto = new UpdateOperatingSystemDto
            {
                Name = "New Name",
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(updateDto),
                Encoding.UTF8,
                "application/json");
            var id = 999;
            var response = await _client.PutAsync($"/api/operatingsystems/{id}", jsonContent);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        public static IEnumerable<object[]> LongStrings =>
    new List<object[]>
    {
        new object[] { new string('A', 101) },
        new object[] { "" },
        new object[] { null! },
        new object[] { " " }
    };

        [Theory]
        [MemberData(nameof(LongStrings))]
        public async Task PutoperatingSystem_ReturnBadRequest_WhenFieldIsEmptyorNull(string name)
        {
            var newOperatingSystem = new CreateOperatingSystemDto
            {
                Name = "Nom vide"
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(newOperatingSystem),
                Encoding.UTF8,
                "application/json");

            // Act
            var postResponse = await _client.PostAsync("/api/operatingsystems", jsonContent);

            // Assert
            postResponse.EnsureSuccessStatusCode();
            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<ReadOperatingSystemDto>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            var id = responseObject!.Id;
            var updateDto = new UpdateOperatingSystemDto
            {
                Name = name,
            };

            jsonContent = new StringContent(
                JsonSerializer.Serialize(updateDto),
                Encoding.UTF8,
                "application/json");
            
            var response = await _client.PutAsync($"/api/operatingsystems/{id}", jsonContent);
            var deleteResponse = await _client.DeleteAsync($"/api/operatingsystems/{id}");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

    }
}
