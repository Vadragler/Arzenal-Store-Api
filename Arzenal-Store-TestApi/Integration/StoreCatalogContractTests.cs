using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Arzenal.Store.Api.TestIntegration.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Arzenal.Store.Api.TestIntegration.Integration;

public class StoreCatalogContractTests : IsolatedApiTest
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public StoreCatalogContractTests() => _factory = Factory;

    [Fact]
    public async Task AnonymousCatalog_ReturnsAnEmptyPageWithoutPrivateApps()
    {
        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/store/v1/apps");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Empty(page.GetProperty("Items").EnumerateArray());
        Assert.Equal(0, page.GetProperty("TotalCount").GetInt32());
    }

    [Fact]
    public async Task OpenApi_DescribesTheRawArtifactUploadBody()
    {
        using var client = _factory.CreateClient();
        var document = await client.GetFromJsonAsync<JsonElement>("/swagger/v1/swagger.json");
        var schema = document.GetProperty("paths")
            .GetProperty("/api/store/v1/admin/releases/{releaseId}/artifacts").GetProperty("post")
            .GetProperty("requestBody").GetProperty("content").GetProperty("application/octet-stream").GetProperty("schema");
        Assert.Equal("binary", schema.GetProperty("format").GetString());
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=101")]
    [InlineData("platform=ios")]
    [InlineData("platform=android&arch=invalid")]
    [InlineData("osVersion=invalid")]
    public async Task Catalog_RejectsInvalidFilters(string query)
    {
        using var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/store/v1/apps?{query}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DraftAppAndRelease_AreNotPublicAndCannotBePublishedWithoutFiles()
    {
        using var manager = _factory.CreateAuthenticatedClient();
        using var anonymous = _factory.CreateClient();
        var slug = $"draft-{Guid.NewGuid():N}";
        var created = await manager.PostAsJsonAsync("/api/store/v1/admin/apps", new { Name = "Private draft", Slug = slug });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var app = await created.Content.ReadFromJsonAsync<JsonElement>();
        var appId = app.GetProperty("Id").GetGuid();
        var releaseResponse = await manager.PostAsJsonAsync($"/api/store/v1/admin/apps/{appId}/releases", new { Version = "1.0.0" });
        Assert.Equal(HttpStatusCode.Created, releaseResponse.StatusCode);
        var release = await releaseResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Draft", release.GetProperty("Status").GetString());
        var releaseId = release.GetProperty("Id").GetGuid();

        Assert.Equal(HttpStatusCode.NotFound, (await anonymous.GetAsync($"/api/store/v1/apps/{slug}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await anonymous.GetAsync($"/api/store/v1/apps/{slug}/releases/latest?platform=windows&arch=x64")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await manager.PostAsync($"/api/store/v1/admin/releases/{releaseId}/publish", null)).StatusCode);
    }

    [Theory]
    [InlineData("/api/store/v1/admin/apps")]
    [InlineData("/api/store/v1/admin/apps/11111111-1111-1111-1111-111111111111/releases")]
    [InlineData("/api/store/v1/admin/releases/11111111-1111-1111-1111-111111111111/publish")]
    public async Task Administration_RejectsAnonymousWrites(string path)
    {
        using var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync(path, new { Name = "Unauthorized", Slug = "unauthorized", Version = "1.0.0" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreatingADuplicateSlugOrVersion_ReturnsConflict()
    {
        using var manager = _factory.CreateAuthenticatedClient();
        var request = new { Name = "Unique", Slug = $"unique-{Guid.NewGuid():N}" };
        var first = await manager.PostAsJsonAsync("/api/store/v1/admin/apps", request);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        var app = await first.Content.ReadFromJsonAsync<JsonElement>();
        var appId = app.GetProperty("Id").GetGuid();
        Assert.Equal(HttpStatusCode.Conflict, (await manager.PostAsJsonAsync("/api/store/v1/admin/apps", request)).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await manager.PostAsJsonAsync($"/api/store/v1/admin/apps/{appId}/releases", new { Version = "1.0.0" })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await manager.PostAsJsonAsync($"/api/store/v1/admin/apps/{appId}/releases", new { Version = "1.0.0" })).StatusCode);
    }

    [Theory]
    [InlineData("POST", "apps")]
    [InlineData("PUT", "apps/11111111-1111-1111-1111-111111111111")]
    [InlineData("GET", "apps/11111111-1111-1111-1111-111111111111/releases")]
    [InlineData("POST", "apps/11111111-1111-1111-1111-111111111111/releases")]
    [InlineData("GET", "apps/11111111-1111-1111-1111-111111111111/audit")]
    [InlineData("GET", "releases/11111111-1111-1111-1111-111111111111")]
    [InlineData("POST", "releases/11111111-1111-1111-1111-111111111111/artifacts")]
    [InlineData("DELETE", "releases/11111111-1111-1111-1111-111111111111/artifacts/22222222-2222-2222-2222-222222222222")]
    [InlineData("POST", "releases/11111111-1111-1111-1111-111111111111/publish")]
    [InlineData("POST", "releases/11111111-1111-1111-1111-111111111111/withdraw")]
    public async Task Administration_RejectsAuthenticatedUsersWithoutTheAdminRole(string method, string path)
    {
        var configuration = _factory.Services.GetRequiredService<IConfiguration>();
        var token = new JwtSecurityToken(issuer: configuration["Jwt:Issuer"], audience: "ArzenalStoreManager",
            claims: new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()), new Claim(ClaimTypes.Role, "User") },
            expires: DateTime.UtcNow.AddMinutes(5), signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!)), SecurityAlgorithms.HmacSha256));
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });
        client.DefaultRequestHeaders.Add("Cookie", $"authToken={new JwtSecurityTokenHandler().WriteToken(token)}");
        using var request = new HttpRequestMessage(new HttpMethod(method), $"/api/store/v1/admin/{path}");
        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(request)).StatusCode);
    }

    [Theory]
    [InlineData("/api/appfiles/icon/11111111-1111-1111-1111-111111111111")]
    [InlineData("/api/appfiles/download/11111111-1111-1111-1111-111111111111/versions/1.0.0/platforms/windows")]
    public async Task LegacyFileRoutes_NoLongerProvideAnAnonymousBypass(string path)
    {
        using var anonymous = _factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(path)).StatusCode);
    }
}
