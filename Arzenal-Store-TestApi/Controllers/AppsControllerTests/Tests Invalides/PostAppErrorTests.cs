using Arzenal.Shared.Dtos.DTOs.AppDto;
using System.Net;
using System.Text;
using System.Text.Json;
using TestApi.Infrastructure;

namespace TestApi.Controllers.AppsControllerTests.Tests_Invalides
{
    [Collection("SharedDatabaseCollection")]
    public class PostAppErrorTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public PostAppErrorTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Theory]
        [InlineData(null, "Version is required", "C:/ New_Test_App", "The Name field is required.")]
        [InlineData("AppName", null, "C:/New_Test_App", "The Version field is required.")]
        [InlineData("AppName", "1.0.0", null, "The FilePath field is required.")]
        public async Task PostApp_ReturnsBadRequest_WhenRequiredFieldsAreMissing(string? name, string? version = null!, string? filepath = null!, string expectedErrorMessage = null!)
        {
            // Arrange
            var newApp = new CreateAppDto
            {
                Name = name!,
                Version = version!,
                FilePath = filepath!,
                Description = "Description",
                IsVisible = true,
                IconePath = "C:/Icons/test.png",
                Requirements = "Windows 10+",
                LastUpdated = DateTime.UtcNow,
                AppSize = 123,
                CategoryId = 1,
                LanguageIds = [1],
                TagIds = [1],
                OsIds = [1]
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(newApp),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/apps", jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectedErrorMessage, content);
        }

        public static IEnumerable<object[]> InvalidReferencesData =>
    [
        [999, new List<int>{1}, new List<int>{1}, new List<int>{1}], // Invalid category
        [1, new List<int>{999}, new List<int>{1}, new List<int>{1}], // Invalid language
        [1, new List<int>{1}, new List<int>{999}, new List<int>{1}], // Invalid OS
        [1, new List<int>{1}, new List<int>{1}, new List<int>{999}]  // Invalid tag
    ];

        [Theory]
        [MemberData(nameof(InvalidReferencesData))]
        public async Task PostApp_ReturnBadRequest_WhenCategoryIdIsInvalid(int categoryId, List<int> languageIds, List<int> osIds, List<int> tagIds)
        {          
            // Corps de la requete
            var newApp = new CreateAppDto
            {
                Name = "New Test App",
                Version = "1.0.1",
                FilePath = "C:/New_Test_App",
                Description = "App de test ajoutée via test POST",
                IsVisible = true,
                IconePath = "C:/Icons/new.png",
                Requirements = "Windows 10+",
                LastUpdated = DateTime.UtcNow,
                AppSize = (long)150.5,
                CategoryId = categoryId,
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
            Assert.Equal(HttpStatusCode.BadRequest, postResponse.StatusCode);
        }
    }
}
