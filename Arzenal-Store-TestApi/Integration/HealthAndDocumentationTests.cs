using Arzenal.Store.Api.TestIntegration.Infrastructure;
using System.Net;

namespace Arzenal.Store.Api.TestIntegration.Integration;

public class HealthAndDocumentationTests
{
    [Fact]
    public async Task Readiness_ReturnsOk_WhenBothDatabasesAreReachable()
    {
        await using var factory = new CustomWebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SwaggerDocument_IsAvailable_InDevelopment()
    {
        await using var factory = new CustomWebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
