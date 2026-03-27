using System.Net;
using System.Threading.Tasks;
using Arzenal.Store.Api.TestIntegration.Infrastructure;
using Xunit;

namespace Arzenal.Store.Api.TestIntegration.Integration.Controllers.CategoriesControllerTests
{
    public class CategoriesAuthTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;

        public CategoriesAuthTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetAll_ReturnsForbidden_WhenAudienceIsWeb()
        {
            // Arrange: use a user-agent that maps to "web" audience in the fake auth handler
            var client = _factory.CreateAuthenticatedClient(userAgent: "Mozilla/5.0");

            // Act
            var response = await client.GetAsync("api/categories");

            // Assert: controller requires ArzenalStoreManager audience + Admin role => should be Forbidden for "web"
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WhenAudienceIsArzenalStoreManager()
        {
            // Arrange: default CreateAuthenticatedClient uses "ArzenalStoreManager" as User-Agent
            var client = _factory.CreateAuthenticatedClient(userAgent: "ArzenalStoreManager");

            // Act
            var response = await client.GetAsync("api/categories");

            // Assert: should succeed
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
