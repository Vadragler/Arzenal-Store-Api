using Arzenal.Shared.Dtos.DTOs.AppDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TestApi.Infrastructure;

namespace TestApi.Controllers.AppsControllerTests.Tests_Valides
{
    [Collection("SharedDatabaseCollection")]
    public class PatchAppSuccessTests
    {

        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public PatchAppSuccessTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        public static IEnumerable<object[]> GetPatchAppData()
        {
            yield return new object[]
            {
        new UpdateAppDto
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
        }
            };

            yield return new object[]
            {
        new UpdateAppDto
        {
            Name = "Partial Name Change",
            Description = "Only description and visibility updated",
            IsVisible = false
            // Les autres champs sont null -> pas modifiés
        }
            };
        }


        [Theory]
        [MemberData(nameof(GetPatchAppData))]
        public async Task PatchApp_ReturnsOk_WhenDataIsValid(UpdateAppDto updateDto)
        {
            // Arrange
            var allResponse = await _client.GetAsync("/api/apps");
            allResponse.EnsureSuccessStatusCode();
            var allContent = await allResponse.Content.ReadAsStringAsync();

            var apps = JsonSerializer.Deserialize<List<ReadAppDto>>(allContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var validAppId = apps!.First().Id;
            var appId = validAppId; // L'ID d'une app déjà présente dans la BDD

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(updateDto),
                Encoding.UTF8,
                "application/json");

            // Act
            var patchResponse = await _client.PatchAsync($"/api/apps/{appId}", jsonContent);

            // Assert
            patchResponse.EnsureSuccessStatusCode();

            // Vérifions que l'app a bien été modifiée comme prévu
            var getResponse = await _client.GetAsync($"/api/apps/{appId}");
            var content = await getResponse.Content.ReadAsStringAsync();
            var updatedApp = JsonSerializer.Deserialize<ReadAppDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.Equal(updateDto.Name, updatedApp!.Name);
            Assert.Equal(updateDto.Description, updatedApp.Description);
            Assert.Equal(updateDto.IsVisible, updatedApp.IsVisible);
        }

    }
}
