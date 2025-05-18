using ArzenalApi.DTOs.OperatingSystemDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TestApi.Infrastructure;

namespace TestApi.Controllers.OperatingSystemsControllerTests.Tests_Invalides
{
    [Collection("SharedDatabaseCollection")]
    public class PostOperatingSystemsErrorTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;


        public PostOperatingSystemsErrorTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
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
        public async Task PostOperatingSystem_ReturnsBadRequest_WhenDataIsInvalid(string _name)
        {
            // Arrange
            // Pour le test POST
            
            
            
            // Corps de la requete
            var newoperatingSystem = new CreateOperatingSystemDto
            {
                Name = _name
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(newoperatingSystem),
                Encoding.UTF8,
                "application/json");

            // Act
            var postResponse = await _client.PostAsync("/api/operatingsystems", jsonContent);

            // Assert
            

            Assert.Equal(HttpStatusCode.BadRequest, postResponse.StatusCode);
        }
    }
}
