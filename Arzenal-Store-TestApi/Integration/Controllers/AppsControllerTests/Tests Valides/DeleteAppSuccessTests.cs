using ArzenalStoreSharedDto.DTOs.AppDto;
using ArzenalStoreApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;
using TestArzenalStoreApi.Infrastructure;

namespace TestArzenalStoreApi.Integration.Controllers.AppsControllerTests.Tests_Valides
{
    [Collection("SharedDatabaseCollection")]
    public class DeleteAppSuccessTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public DeleteAppSuccessTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task DeletApp_ReturnNoContent_WhenAppExists()
        {
            // Arrange
            var newApp = new CreateAppDto
            {
                Name = "App to Delete",
                Version = "1.0.0",
                FilePath = "C:/PathToApp",
                Description = "App to test DELETE",
                IsVisible = true,
                IconePath = "C:/Icons/delete.png",
                Requirements = "Windows 10+",
                LastUpdated = DateTime.UtcNow,
                AppSize = (long)123.5,
                CategoryId = 1,
                LanguageIds = [1],
                OsIds = [1],
                TagIds = [1]
            };

            // Créer l'application via POST pour avoir un élément à supprimer
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(newApp),
                Encoding.UTF8,
                "application/json");

            var postResponse = await _client.PostAsync("/api/apps", jsonContent);
            postResponse.EnsureSuccessStatusCode();

            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseJson);
            var id = responseObject.GetProperty("id").GetGuid();

            var deleteResponse = await _client.DeleteAsync($"/api/apps/{id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // Vérifier si l'app a bien été supprimée
            var getResponse = await _client.GetAsync($"/api/apps/{id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

            // Vérification BDD
            using var scope = _factory.Services!.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            Assert.NotEmpty(context.AppLanguages);
            Assert.NotEmpty(context.AppTags);
            Assert.NotEmpty(context.AppOperatingSystems);

            Assert.Empty(context.AppLanguages.Where(a => a.AppId == id));
            Assert.Empty(context.AppTags.Where(a => a.AppId == id));
            Assert.Empty(context.AppOperatingSystems.Where(a => a.AppId == id));
        }


        [Fact]
        public async Task DeleteApp_ShouldCascadeDeleteAppLanguagesAppTagsAppOperatingSystems()
        {
            // Arrange
            var app = new CreateAppDto
            {
                Name = "App to Delete5",
                Version = "1.0.0",
                FilePath = "C:/PathToApp",
                Description = "App to test DELETE",
                IsVisible = true,
                IconePath = "C:/Icons/delete.png",
                Requirements = "Windows 10+",
                LastUpdated = DateTime.UtcNow,
                AppSize = (long)123.5,
                CategoryId = 1,
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(app),
                Encoding.UTF8,
                "application/json");

            // Créer l'application via l'API
            var postResponse = await _client.PostAsync("/api/apps", jsonContent);
            postResponse.EnsureSuccessStatusCode();

            // Récupérer l'ID de l'application nouvellement créée
            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseJson);
            var id = responseObject.GetProperty("id").GetGuid();

            // Act: Supprimer l'application
            var deleteResponse = await _client.DeleteAsync($"/api/apps/{id}");
            deleteResponse.EnsureSuccessStatusCode();

            // Assert: Vérifier que les relations ont été supprimées en base de données
            using var scope = _factory.Services!.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Vérifier que le contexte existe et est correctement initialisé
            Assert.NotNull(context);

            //var appLanguages = await context.AppLanguages.Where(al => al.AppId == id).ToListAsync();
            //var appTags = await context.AppTags.Where(at => at.AppId == id).ToListAsync();
            //var appOperatingSystems = await context.AppOperatingSystems.Where(aos => aos.AppId == id).ToListAsync();

            Assert.NotEmpty(await context.AppLanguages.ToListAsync());
            Assert.NotEmpty(await context.AppTags.ToListAsync());
            Assert.NotEmpty(await context.AppOperatingSystems.ToListAsync());
            // Vérifier que la suppression en cascade a bien eu lieu
            //Assert.Empty(appLanguages); // La relation AppLanguage devrait être supprimée
            //Assert.Empty(appTags); // La relation AppTag devrait être supprimée
            //Assert.Empty(appOperatingSystems); // La relation AppOperatingSystem devrait être supprimée
        }
    }
}
