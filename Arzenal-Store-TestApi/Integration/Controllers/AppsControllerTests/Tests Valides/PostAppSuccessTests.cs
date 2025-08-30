using ArzenalStoreSharedDto.DTOs.AppDto;
using System.Text;
using System.Text.Json;
using TestArzenalStoreApi.Infrastructure;

namespace TestArzenalStoreApi.Integration.Controllers.AppsControllerTests.Tests_Valides
{
    [Collection("SharedDatabaseCollection")]
    public class PostAppSuccessTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public PostAppSuccessTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task PostApp_ReturnsCreated_WhenDataIsValid()
        {
            // Arrange
            // Pour le test POST
            var categoryId = 1;
            var languageIds = new List<int> { 1 };
            var tagIds = new List<int> { 1 };
            var osIds = new List<int> { 1 };
            var _name = "New Test App6544256126169";
            var _version = "1.0.1";
            var _filePath = "C:/New_Test_App";
            var _description = "App de test ajoutée via test POST";
            var _IsVisible = true;
            var _iconePath = "C:/Icons/new.png";
            var _requirements = "Windows 10+";
            var _releaseDate = DateTime.UtcNow;
            var _lastUpdated = DateTime.UtcNow;
            var _appSize = (long)150.5;
            // Corps de la requete
            var newApp = new CreateAppDto
            {
                Name = _name,
                Version = _version,
                FilePath = _filePath,
                Description = _description,
                IsVisible = _IsVisible,
                IconePath = _iconePath,
                Requirements = _requirements,
                LastUpdated = _lastUpdated,
                AppSize = _appSize,
                CategoryId= categoryId,
                LanguageIds = languageIds,
                TagIds = tagIds,
                OsIds = osIds
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(newApp),
                Encoding.UTF8,
                "application/json");

            // Act
            var postResponse = await _client.PostAsync("/api/apps", jsonContent);

            // Assert
            postResponse.EnsureSuccessStatusCode();
            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseJson);
            var id = responseObject.GetProperty("id").GetGuid();

            //Vérifié que l'appli a bien été ajouté
            var getAllAfterPost = await _client.GetAsync($"/api/apps/{id}");
            getAllAfterPost.EnsureSuccessStatusCode();
            var newContent = await getAllAfterPost.Content.ReadAsStringAsync();
            var updatedApps = JsonSerializer.Deserialize<ReadAppDto>(newContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });

            Assert.Equal(_name, updatedApps!.Name);
            Assert.Equal(_version, updatedApps.Version);
            Assert.Equal(_filePath, updatedApps.FilePath);
            Assert.Equal(_description, updatedApps.Description);
            Assert.Equal(_IsVisible, updatedApps.IsVisible);
            Assert.Equal(_iconePath, updatedApps.IconePath);
            Assert.Equal(_requirements, updatedApps.Requirements);
            //Assert.Equal(_releaseDate, updatedApps.ReleaseDate);
            //Assert.Equal(_lastUpdated, updatedApps.LastUpdated);
            Assert.Equal(_appSize, updatedApps.AppSize);
            Assert.Equal("Software", updatedApps.Category);
            Assert.Equal(["English"], updatedApps.Languages);
            Assert.Equal(["Productivity"], updatedApps.Tags);
            Assert.Equal(["Windows"], updatedApps.OperatingSystems);
        }
    }
}
