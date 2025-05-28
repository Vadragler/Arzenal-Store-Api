using Arzenal.Shared.Dtos.DTOs.OperatingSystemDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TestApi.Infrastructure;

namespace TestApi.Controllers.OperatingSystemsControllerTests.Tests_Valides
{
    [Collection("SharedDatabaseCollection")]
    public class GetOperatingSystemsSuccessTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;


        public GetOperatingSystemsSuccessTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }
        [Fact]
        public async Task GetOperatingSystem_ReturnsList_WhenOsExist()
        {
            var response = await _client.GetAsync("/api/operatingsystems");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var osList = JsonSerializer.Deserialize<List<ReadOperatingSystemDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(osList);
            Assert.NotEmpty(osList);
            Assert.Contains(osList, os => os.Name == "Windows");
        }

        [Fact]
        public async Task GetOperatingSystemById_ReturnsList_WhenIdIsValid()
        {
            var id = 1;
            var response = await _client.GetAsync($"/api/operatingsystems/{id}");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var osList = JsonSerializer.Deserialize<ReadOperatingSystemDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

           
            Assert.Equal("Windows", osList!.Name);
        }
    }
}
