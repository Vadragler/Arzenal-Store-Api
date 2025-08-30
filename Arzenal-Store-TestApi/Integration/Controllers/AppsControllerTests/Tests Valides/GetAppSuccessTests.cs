using ArzenalStoreSharedDto.DTOs.AppDto;
using System.Text.Json;
using TestArzenalStoreApi.Infrastructure;


namespace TestArzenalStoreApi.Integration.Controllers.AppsControllerTests.Tests_Valides
{
    [Collection("SharedDatabaseCollection")]
    public class GetAppSuccessTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public GetAppSuccessTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
             _client = factory.CreateClient();
             
        }

        [Fact]
        public async Task GetAllApps_ReturnsOk()
        {
            // Arrange
            var response = await _client.GetAsync("/api/apps");
            
            // Act & Assert
            response.EnsureSuccessStatusCode(); // Vérifie si la réponse est un code 2xx (OK)
        }


        [Fact]
        public async Task GetAllApps_ReturnsValidAppData()
        {
            // Arrange
            var response = await _client.GetAsync("/api/apps");

            // Act
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var apps = JsonSerializer.Deserialize<List<ReadAppDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            Assert.NotNull(apps);
            Assert.NotEmpty(apps);
            Assert.All(apps, app =>
            {
                Assert.NotNull(app.Name);
                Assert.NotNull(app.Version);
                Assert.NotNull(app.FilePath);
                Assert.NotNull(app.Category);
                //Assert.NotEmpty(app.Languages!);
                //Assert.NotEmpty(app.Tags!);
                //Assert.NotEmpty(app.OperatingSystems!);
            });
        }

        [Fact]
        public async Task GetAppById_ReturnsCorrectApp()
        {
            // Arrange
            var allResponse = await _client.GetAsync("/api/apps");
            allResponse.EnsureSuccessStatusCode();
            var allContent = await allResponse.Content.ReadAsStringAsync();

            var apps = JsonSerializer.Deserialize<List<ReadAppDto>>(allContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(apps);  // Vérifie que la liste d'applications n'est pas nulle
            Assert.NotEmpty(apps);  // Vérifie que la liste contient des éléments

            var targetApp = apps!.First();  // On prend la première application

            // Act
            var response = await _client.GetAsync($"/api/apps/{targetApp.Id}");
            var content = await response.Content.ReadAsStringAsync();
            var app = JsonSerializer.Deserialize<ReadAppDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.NotNull(app);  // Vérifie que l'application récupérée n'est pas nulle
            Assert.Equal(targetApp.Id, app!.Id);
            Assert.Equal(targetApp.Name, app.Name);
        }

    }
}
