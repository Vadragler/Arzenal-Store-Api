using ArzenalStoreSharedDto.DTOs.AppDto;
using System.Net;
using System.Text;
using System.Text.Json;
using TestArzenalStoreApi.Infrastructure;

namespace TestArzenalStoreApi.Integration.Controllers.AppsControllerTests.Tests_Invalides
{
    [Collection("SharedDatabaseCollection")]
    public class PatchAppErrorTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;


        public PatchAppErrorTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task PatchApp_ReturnConflict_WhenAppAlreadyExist()
        {
            // Arrange
            var newApp = new CreateAppDto {
                Name = "New Name Alredy Exist",
                Description = "Updated description",
                IsVisible = true,
                Version = "2.0",
                FilePath = "C:/Updated.exe",
                IconePath = "C:/Icons/updated.png",
                Requirements = "Windows 11",
                AppSize = 250,
                LastUpdated = DateTime.UtcNow,
                CategoryId = 1,

            };
            var jsonContent = new StringContent(
               JsonSerializer.Serialize(newApp),
               Encoding.UTF8,
               "application/json");
            // Act
            var postResponse = await _client.PostAsync("/api/apps", jsonContent);
            postResponse.EnsureSuccessStatusCode();
            newApp = new CreateAppDto
            {
                Name = "New Name Not Alredy Exist",
                Description = "Updated description",
                IsVisible = true,
                Version = "2.0",
                FilePath = "C:/Updated.exe",
                IconePath = "C:/Icons/updated.png",
                Requirements = "Windows 11",
                AppSize = 250,
                LastUpdated = DateTime.UtcNow,
                CategoryId = 1,

            };
            jsonContent = new StringContent(
               JsonSerializer.Serialize(newApp),
               Encoding.UTF8,
               "application/json");
            // Act
            postResponse = await _client.PostAsync("/api/apps", jsonContent);
            postResponse.EnsureSuccessStatusCode();
            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<ReadAppDto>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            var id = responseObject!.Id;
            var updatedApp = new UpdateAppDto { Name = "New Name Alredy Exist" ,
                Version = "2.0",
                FilePath = "C:/Updated.exe"
            };
            jsonContent = new StringContent(
                JsonSerializer.Serialize(updatedApp),
                Encoding.UTF8,
                "application/json");
            var putResponse = await _client.PatchAsync($"/api/apps/{id}", jsonContent);
            // Assert
            Assert.Equal(HttpStatusCode.Conflict, putResponse.StatusCode);
        }

        // ID malformé
        [Fact]
        public async Task PatchApp_ReturnBadRequest_WhenIdIsMalformed()
        {
            var updateDto = new UpdateAppDto
            {
                Name = "New Name",
                Description = "Updated description",
                IsVisible = true,
                Version = "2.0",
                FilePath = "C:/Updated.exe",
                IconePath = "C:/Icons/updated.png",
                Requirements = "Windows 11",
                AppSize = 250,
                LastUpdated = DateTime.UtcNow,
                CategoryId = 1,
                LanguageIds = [1],
                OsIds = [1],
                TagIds = [1]
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(updateDto),
                Encoding.UTF8,
                "application/json");
            var id = "abc";
            var response = await _client.PatchAsync($"/api/apps/{id}", jsonContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PatchApp_ReturnBadRequest_WhenIdIsInt()
        {
            var updateDto = new UpdateAppDto
            {
                Name = "New Name",
                Description = "Updated description",
                IsVisible = true,
                Version = "2.0",
                FilePath = "C:/Updated.exe",
                IconePath = "C:/Icons/updated.png",
                Requirements = "Windows 11",
                AppSize = 250,
                LastUpdated = DateTime.UtcNow,
                CategoryId = 1,
                LanguageIds = [1],
                OsIds = [1],
                TagIds = [1]
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(updateDto),
                Encoding.UTF8,
                "application/json");
            var id = 1;
            var response = await _client.PatchAsync($"/api/apps/{id}", jsonContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PatchApp_ReturnNotFound_WhenIdDoesNotExist()
        {
            var updateDto = new UpdateAppDto
            {
                Name = "New Name",
                Description = "Updated description",
                IsVisible = true,
                Version = "2.0",
                FilePath = "C:/Updated.exe",
                IconePath = "C:/Icons/updated.png",
                Requirements = "Windows 11",
                AppSize = 250,
                LastUpdated = DateTime.UtcNow,
                CategoryId = 1,

            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(updateDto),
                Encoding.UTF8,
                "application/json");
            var id = Guid.NewGuid();
            var response = await _client.PatchAsync($"/api/apps/{id}", jsonContent);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

    }
}
