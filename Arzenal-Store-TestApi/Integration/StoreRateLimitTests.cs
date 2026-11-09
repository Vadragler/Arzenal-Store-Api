using System.Net;
using Arzenal.Store.Api.TestIntegration.Infrastructure;

namespace Arzenal.Store.Api.TestIntegration.Integration;

public class StoreRateLimitTests
{
    [Fact]
    public async Task StoreUpload_RejectsRequestsAboveTheRealAdminQuota()
    {
        await using var factory = new CustomWebApplicationFactory<Program>();
        using var manager = factory.CreateAuthenticatedClient();
        var endpoint = $"/api/store/v1/admin/releases/{Guid.NewGuid()}/artifacts";
        for (var index = 0; index < 12; index++)
        {
            using var content = new ByteArrayContent([1]);
            content.Headers.ContentType = new("application/octet-stream");
            using var response = await manager.PostAsync(endpoint, content);
            Assert.NotEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
        }
        using var lastContent = new ByteArrayContent([1]);
        lastContent.Headers.ContentType = new("application/octet-stream");
        using var limited = await manager.PostAsync(endpoint, lastContent);
        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
    }
}
