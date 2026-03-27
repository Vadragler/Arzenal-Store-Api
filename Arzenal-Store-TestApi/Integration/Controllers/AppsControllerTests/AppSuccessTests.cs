using ArzenalStoreInfrastructure.Data;
using Arzenal.Dto.DTOs.AppDto;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.Text.Json;
using Arzenal.Store.Api.TestIntegration.Infrastructure;

namespace Arzenal.Store.Api.TestIntegration.Integration.Controllers.AppsControllerTests
{
    public class AppSuccessTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public AppSuccessTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateAuthenticatedClient();
        }

        // ---------- POST ----------
        [Fact]
        public async Task PostApp_ReturnsCreated_WhenDataIsValid()
        {
            // Arrange
            var dto = new CreateAppDto
            {
                Name = "Test App",
                Version = "1.0",
                Description = "Desc",
                IsVisible = true,
                IconePath = "C:/icon.png",
                Requirements = "Win10+",
                LastUpdated = DateTime.UtcNow,
                AppSize = 100,
                CategoryId = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"),
                LanguageIds = [Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d")],
                TagIds = [Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d")],
                OsIds = [Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d")]
            };

            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

            // Act
            var post = await _client.PostAsync("api/apps", content);

            post.EnsureSuccessStatusCode();

            // Assert
            Assert.Equal(HttpStatusCode.Created, post.StatusCode);
            //Deserialize response to get the created app ID
            var json = await post.Content.ReadAsStringAsync();
            var createdApp = JsonSerializer.Deserialize<ReadAppDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(createdApp);
            Assert.Equal(dto.Name, createdApp!.Name);
            Assert.Equal(dto.Version, createdApp.Version);
            Assert.Equal(dto.Description, createdApp.Description);
            Assert.Equal(dto.IsVisible, createdApp.IsVisible);
            Assert.Equal(dto.IconePath, createdApp.IconePath);
            Assert.Equal(dto.Requirements, createdApp.Requirements);
            Assert.Equal(dto.AppSize, createdApp.AppSize);
            Assert.Equal("Software", createdApp.Category); // si tu exposes CategoryId dans ReadAppDto
            Assert.Equal(["English"], createdApp.Languages);
            Assert.Equal(["Productivity"], createdApp.Tags);
            Assert.Equal(["Windows"], createdApp.OperatingSystems);
        }

        // ---------- GET ----------
        [Fact]
        public async Task GetAllApps_ReturnsOk_AndNotEmpty()
        {
            // Act
            var response = await _client.GetAsync("api/apps");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var apps = JsonSerializer.Deserialize<List<ReadAppDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Assert
            Assert.NotNull(apps);
            Assert.NotEmpty(apps);
        }

        [Fact]
        public async Task GetAppById_ReturnsSameApp()
        {
            // Act
            var all = await _client.GetAsync("api/apps");
            var list = JsonSerializer.Deserialize<List<ReadAppDto>>(await all.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            var id = list.First().Id;

            var get = await _client.GetAsync($"api/apps/{id}");
            get.EnsureSuccessStatusCode();

            var app = JsonSerializer.Deserialize<ReadAppDto>(await get.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Assert
            Assert.Equal(id, app!.Id);
        }

        // ---------- PATCH ----------
        [Fact]
        public async Task PatchApp_UpdatesFields()
        {
            // Act
            var all = await _client.GetAsync("api/apps");
            var list = JsonSerializer.Deserialize<List<ReadAppDto>>(await all.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            var id = list.First().Id;

            var update = new UpdateAppDto { Name = "Updated", Description = "Changed", IsVisible = false };
            var content = new StringContent(JsonSerializer.Serialize(update), Encoding.UTF8, "application/json");

            var patch = await _client.PatchAsync($"api/apps/{id}", content);
            patch.EnsureSuccessStatusCode();

            var get = await _client.GetAsync($"api/apps/{id}");
            var app = JsonSerializer.Deserialize<ReadAppDto>(await get.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Assert
            Assert.Equal("Updated", app!.Name);
        }

        // ---------- DELETE ----------
        [Fact]
        public async Task DeleteApp_RemovesAppAndRelations()
        {
            // Arrange
            var dto = new CreateAppDto
            {
                Name = "Delete Me",
                Version = "1.0",
                Description = "Del Test",
                IsVisible = true,
                IconePath = "C:/icon.png",
                Requirements = "Win10+",
                LastUpdated = DateTime.UtcNow,
                AppSize = 100,
                CategoryId = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"),
                LanguageIds = [Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d")],
                TagIds = [Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d")],
                OsIds = [Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d")]
            };

            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

            // Act & Assert
            var post = await _client.PostAsync("api/apps", content);
            var json = await post.Content.ReadAsStringAsync();
            var id = JsonSerializer.Deserialize<JsonElement>(json).GetProperty("Id").GetGuid();

            var delete = await _client.DeleteAsync($"api/apps/{id}");
            Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

            var get = await _client.GetAsync($"api/apps/{id}");
            Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);

            using var scope = _factory.Services.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.Empty(ctx.AppLanguages.Where(x => x.AppId == id));
            Assert.Empty(ctx.AppTags.Where(x => x.AppId == id));
            Assert.Empty(ctx.AppOperatingSystems.Where(x => x.AppId == id));
        }
    }
}
