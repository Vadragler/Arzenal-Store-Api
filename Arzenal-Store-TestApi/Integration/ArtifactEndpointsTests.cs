using Arzenal.Store.Api.TestIntegration.Infrastructure;
using Arzenal.Dto.DTOs.AppDto;
using Arzenal.Dto.DTOs.Pagination;
using ArzenalStoreInfrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Arzenal.Store.Api.TestIntegration.Integration;

public class ArtifactEndpointsTests
{
    private static readonly Guid SeededAppId = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d");

    [Fact]
    public async Task UploadThenDownload_UsesTheExactVersionAndPlatformArtifact()
    {
        await using var factory = new CustomWebApplicationFactory<Program>();
        using var client = factory.CreateAuthenticatedClient();
        var content = new ByteArrayContent([0x4D, 0x5A, 0x00, 0x01]);
        content.Headers.ContentType = new("application/octet-stream");

        var upload = await client.PostAsync(
            $"api/appfiles/upload/apps/{SeededAppId}/versions/1.0.0/files/app/filename/setup.exe/windows",
            content);
        Assert.Equal(HttpStatusCode.Created, upload.StatusCode);

        var download = await client.GetAsync(
            $"api/appfiles/download/{SeededAppId}/versions/1.0.0/platforms/windows");
        Assert.Equal(HttpStatusCode.OK, download.StatusCode);
        Assert.Equal([0x4D, 0x5A, 0x00, 0x01], await download.Content.ReadAsByteArrayAsync());

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var artifact = await db.AppArtifacts.SingleAsync();
        Assert.Equal("1.0.0", artifact.Version);
        Assert.Equal("windows", artifact.Platform);
        Assert.Equal("setup.exe", artifact.OriginalFileName);
    }

    [Fact]
    public async Task CatalogPage_IsBoundedAndProvidesPaginationMetadata()
    {
        await using var factory = new CustomWebApplicationFactory<Program>();
        using var client = factory.CreateAuthenticatedClient();

        var response = await client.GetAsync("api/apps/page?page=1&pageSize=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResultDto<ReadAppDto>>();
        Assert.NotNull(page);
        Assert.Equal(1, page.Page);
        Assert.Equal(1, page.PageSize);
        Assert.True(page.TotalCount >= 2);
        Assert.Single(page.Items);
    }
}
