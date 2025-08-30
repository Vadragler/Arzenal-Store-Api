using ArzenalStoreSharedDto.DTOs.CategorieDto;
using System.Net;
using System.Text;
using System.Text.Json;
using TestArzenalStoreApi.Infrastructure;

namespace TestArzenalStoreApi.Integration.Controllers.CategoriesControllerTests.Tests_Invalides
{
    [Collection("SharedDatabaseCollection")]
    public class PostCategoriesErrorTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public PostCategoriesErrorTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task PostCategorie_ReturnBadRequest_WhenCategorieAlreadyExist()
        {
            // Arrange
            var existingCategorie = new CreateCategorieDto { Name = "Software" };
            var jsonContent = new StringContent(
               JsonSerializer.Serialize(existingCategorie),
               Encoding.UTF8,
               "application/json");
            // Act
            var postResponse = await _client.PostAsync("/api/categories", jsonContent);
            
            // Assert
            Assert.Equal(HttpStatusCode.Conflict, postResponse.StatusCode);
        }


        public static IEnumerable<object[]> LongStrings =>
        [
            [new string('A', 101)],
            [""],
            [null!],
            [" "]
        ];

        [Theory]
        [MemberData(nameof(LongStrings))]
        public async Task PostCategorie_ReturnsBadRequest_WhenDataIsInvalid(string _name)
        {
            // Arrange
            // Pour le test POST



            // Corps de la requete
            var newLanguage = new CreateCategorieDto
            {
                Name = _name
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(newLanguage),
                Encoding.UTF8,
                "application/json");

            // Act
            var postResponse = await _client.PostAsync("/api/categories", jsonContent);

            // Assert


            Assert.Equal(HttpStatusCode.BadRequest, postResponse.StatusCode);
        }
    }
}
