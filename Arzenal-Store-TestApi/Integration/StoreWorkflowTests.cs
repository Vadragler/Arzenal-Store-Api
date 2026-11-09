using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Arzenal.Store.Api.TestIntegration.Infrastructure;
using ArzenalStoreInfrastructure.Configurations;
using ArzenalStoreInfrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;

namespace Arzenal.Store.Api.TestIntegration.Integration;

public sealed class StoreTestFactory : CustomWebApplicationFactory<Program>
{
    private readonly RSA _publisher = RSA.Create(3072);
    private readonly Lazy<HttpClient> _manager;
    public StoreTestFactory() => _manager = new(() => CreateAuthenticatedClient());
    public HttpClient Manager => _manager.Value;
    public string Sign(byte[] bytes) => Convert.ToBase64String(_publisher.SignData(bytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pss));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services =>
        {
            services.PostConfigure<StoreDistributionSettings>(settings =>
                settings.PublisherPublicKeys["integration"] = _publisher.ExportSubjectPublicKeyInfoPem());
            // Une même identité effectue tous les scénarios. Le quota réel possède son propre test.
            services.RemoveAll<IConfigureOptions<RateLimiterOptions>>();
            services.Configure<RateLimiterOptions>(options =>
            {
                foreach (var policy in new[] { "login", "refresh", "invite", "app-login", "upload", "store-read", "store-download" })
                    options.AddPolicy(policy, _ => RateLimitPartition.GetNoLimiter("workflow-tests"));
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_manager.IsValueCreated) _manager.Value.Dispose();
            _publisher.Dispose();
        }
        base.Dispose(disposing);
    }
}

public class StoreWorkflowTests : IsolatedApiTest<StoreTestFactory>
{
    private readonly StoreTestFactory _factory;
    public StoreWorkflowTests() => _factory = Factory;

    [Fact]
    public async Task PublishedArtifact_IsDiscoverableAndDownloadableWithHashRangeAndEtag()
    {
        var (appId, slug, releaseId) = await CreateAsync();
        var bytes = Msix();
        var artifact = await UploadAsync(releaseId, bytes);
        var artifactId = artifact.GetProperty("Id").GetGuid();
        using var anonymous = _factory.CreateClient();
        Assert.Equal(HttpStatusCode.NotFound, (await anonymous.GetAsync($"/api/store/v1/artifacts/{artifactId}/download")).StatusCode);
        await PublishAsync(releaseId);

        var response = await anonymous.GetAsync($"/api/store/v1/apps/{slug}/releases/latest?platform=windows&arch=x64&osVersion=10.0.22000.0");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var latest = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(appId, latest.GetProperty("ApplicationId").GetGuid());
        Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(bytes)), latest.GetProperty("Sha256").GetString());
        Assert.Equal("rsa-pss-sha256", latest.GetProperty("Signature").GetProperty("Type").GetString());
        Assert.DoesNotContain("RelativePath", latest.GetRawText());
        Assert.DoesNotContain(_factory.StoragePath, latest.GetRawText());
        var endpoint = latest.GetProperty("DownloadEndpoint").GetString();
        var download = await anonymous.GetAsync(endpoint);
        Assert.Equal(HttpStatusCode.OK, download.StatusCode);
        Assert.Equal(bytes, await download.Content.ReadAsByteArrayAsync());
        Assert.NotNull(download.Headers.ETag);
        var rangeRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        rangeRequest.Headers.Range = new RangeHeaderValue(0, 9);
        rangeRequest.Headers.IfRange = new RangeConditionHeaderValue(download.Headers.ETag);
        var range = await anonymous.SendAsync(rangeRequest);
        Assert.Equal(HttpStatusCode.PartialContent, range.StatusCode);
        Assert.Equal(bytes[..10], await range.Content.ReadAsByteArrayAsync());
        var cached = new HttpRequestMessage(HttpMethod.Get, endpoint);
        cached.Headers.IfNoneMatch.Add(download.Headers.ETag);
        Assert.Equal(HttpStatusCode.NotModified, (await anonymous.SendAsync(cached)).StatusCode);

        var page = await anonymous.GetFromJsonAsync<JsonElement>($"/api/store/v1/apps?q={slug}&platform=windows&arch=x64&pageSize=1");
        Assert.Equal(1, page.GetProperty("TotalCount").GetInt32());
        Assert.Single(page.GetProperty("Items").EnumerateArray());
        var audit = await _factory.Manager.GetFromJsonAsync<JsonElement>($"/api/store/v1/admin/apps/{appId}/audit");
        Assert.Contains(audit.EnumerateArray(), entry => entry.GetProperty("Action").GetString() == "release-published");
        Assert.All(audit.EnumerateArray(), entry => Assert.False(string.IsNullOrWhiteSpace(entry.GetProperty("ActorId").GetString())));
    }

    [Fact]
    public async Task Withdrawal_RemovesEveryPublicAccessAndDoesNotAllowReplacement()
    {
        var (_, slug, releaseId) = await CreateAsync();
        var bytes = Msix();
        var artifact = await UploadAsync(releaseId, bytes);
        await PublishAsync(releaseId);
        Assert.Equal(HttpStatusCode.OK, (await _factory.Manager.PostAsync($"/api/store/v1/admin/releases/{releaseId}/withdraw", null)).StatusCode);
        using var anonymous = _factory.CreateClient();
        Assert.Equal(HttpStatusCode.NotFound, (await anonymous.GetAsync($"/api/store/v1/apps/{slug}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await anonymous.GetAsync(artifact.GetProperty("DownloadEndpoint").GetString())).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await SendUploadAsync(releaseId, bytes)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await _factory.Manager.PostAsync($"/api/store/v1/admin/releases/{releaseId}/publish", null)).StatusCode);
    }

    [Fact]
    public async Task PublishedArtifacts_CannotBeOverwrittenDeletedOrDeletedThroughLegacyRoutes()
    {
        var (appId, _, releaseId) = await CreateAsync();
        var bytes = Msix();
        var artifact = await UploadAsync(releaseId, bytes);
        await PublishAsync(releaseId);
        Assert.Equal(HttpStatusCode.Conflict, (await SendUploadAsync(releaseId, bytes)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await _factory.Manager.DeleteAsync($"/api/store/v1/admin/releases/{releaseId}/artifacts/{artifact.GetProperty("Id").GetGuid()}")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await _factory.Manager.DeleteAsync($"/api/apps/{appId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await _factory.Manager.DeleteAsync($"/api/appfiles/{appId}")).StatusCode);
        using var anonymous = _factory.CreateClient();
        Assert.Equal(bytes, await anonymous.GetByteArrayAsync(artifact.GetProperty("DownloadEndpoint").GetString()));
    }

    [Fact]
    public async Task Compatibility_DoesNotReturnAnotherArchitectureOrAnUnsupportedOs()
    {
        var (_, slug, releaseId) = await CreateAsync();
        await UploadAsync(releaseId, Msix());
        await PublishAsync(releaseId);
        using var anonymous = _factory.CreateClient();
        Assert.Equal(HttpStatusCode.NotFound, (await anonymous.GetAsync($"/api/store/v1/apps/{slug}/releases/latest?platform=windows&arch=arm64")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await anonymous.GetAsync($"/api/store/v1/apps/{slug}/releases/latest?platform=windows&arch=x64&osVersion=10.0.10000.0")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await anonymous.GetAsync($"/api/store/v1/apps/{slug}/releases/latest?platform=android&arch=arm64")).StatusCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Upload_RejectsAnUntrustedSignatureOrMalformedPackage(bool invalidSignature)
    {
        var (_, _, releaseId) = await CreateAsync();
        var bytes = invalidSignature ? Msix() : Encoding.UTF8.GetBytes("not an archive");
        var response = await SendUploadAsync(releaseId, bytes, invalidSignature ? Convert.ToBase64String(new byte[384]) : null);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var release = await _factory.Manager.GetFromJsonAsync<JsonElement>($"/api/store/v1/admin/releases/{releaseId}");
        Assert.Empty(release.GetProperty("Artifacts").EnumerateArray());
        Assert.Equal("Draft", release.GetProperty("Status").GetString());
    }

    [Fact]
    public async Task Publish_RejectsMissingOrChangedFiles()
    {
        var (_, _, releaseId) = await CreateAsync();
        var bytes = Msix();
        var artifact = await UploadAsync(releaseId, bytes);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var id = artifact.GetProperty("Id").GetGuid();
        var stored = await db.AppArtifacts.AsNoTracking().SingleAsync(item => item.Id == id);
        var path = Path.Combine(_factory.StoragePath, stored.RelativePath);
        bytes[^1] ^= 1;
        await File.WriteAllBytesAsync(path, bytes);
        Assert.Equal(HttpStatusCode.Conflict, (await _factory.Manager.PostAsync($"/api/store/v1/admin/releases/{releaseId}/publish", null)).StatusCode);
        File.Delete(path);
        Assert.Equal(HttpStatusCode.Conflict, (await _factory.Manager.PostAsync($"/api/store/v1/admin/releases/{releaseId}/publish", null)).StatusCode);
    }

    [Fact]
    public async Task DraftArtifact_CanBeRemovedAndUploadedAgain()
    {
        var (_, _, releaseId) = await CreateAsync();
        var bytes = Msix();
        var artifact = await UploadAsync(releaseId, bytes);
        Assert.Equal(HttpStatusCode.NoContent, (await _factory.Manager.DeleteAsync($"/api/store/v1/admin/releases/{releaseId}/artifacts/{artifact.GetProperty("Id").GetGuid()}")).StatusCode);
        await UploadAsync(releaseId, bytes);
        await PublishAsync(releaseId);
    }

    [Fact]
    public async Task HiddenApp_IsNotDownloadableEvenWithPublishedReleases()
    {
        var (appId, slug, releaseId) = await CreateAsync();
        var artifact = await UploadAsync(releaseId, Msix());
        await PublishAsync(releaseId);
        var hidden = await _factory.Manager.PutAsJsonAsync($"/api/store/v1/admin/apps/{appId}", new { Name = slug, Slug = slug, IsVisible = false });
        Assert.Equal(HttpStatusCode.OK, hidden.StatusCode);
        using var anonymous = _factory.CreateClient();
        Assert.Equal(HttpStatusCode.NotFound, (await anonymous.GetAsync(artifact.GetProperty("DownloadEndpoint").GetString())).StatusCode);
    }

    [Fact]
    public async Task ConcurrentUploads_KeepOnlyOneImmutableVariant()
    {
        var (_, _, releaseId) = await CreateAsync();
        var bytes = Msix();
        var results = await Task.WhenAll(SendUploadAsync(releaseId, bytes), SendUploadAsync(releaseId, bytes));
        Assert.Single(results, response => response.StatusCode == HttpStatusCode.Created);
        Assert.Single(results, response => response.StatusCode == HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AndroidArtifact_UsesTheApiLevelAndPreservesItsVersionCode()
    {
        var (_, slug, releaseId) = await CreateAsync();
        using var output = new MemoryStream();
        using (var zip = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        using (var writer = new StreamWriter(zip.CreateEntry("AndroidManifest.xml").Open()))
            writer.Write("inert Android fixture, not an installable APK");
        var bytes = output.ToArray();
        using var request = new HttpRequestMessage(HttpMethod.Post,
            $"/api/store/v1/admin/releases/{releaseId}/artifacts?platform=android&architecture=arm64&packageType=apk&packageId=ovh.arzenal.test&packageVersion=1.0.0&minimumOsVersion=26&androidVersionCode=100");
        request.Content = new ByteArrayContent(bytes);
        request.Content.Headers.ContentType = new("application/octet-stream");
        request.Headers.Add("X-Publisher-Key-Id", "integration");
        request.Headers.Add("X-Artifact-Signature", _factory.Sign(bytes));
        using var uploaded = await _factory.Manager.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, uploaded.StatusCode);
        await PublishAsync(releaseId);
        using var anonymous = _factory.CreateClient();
        var latest = await anonymous.GetFromJsonAsync<JsonElement>($"/api/store/v1/apps/{slug}/releases/latest?platform=android&arch=arm64&osVersion=35");
        Assert.Equal(100, latest.GetProperty("AndroidVersionCode").GetInt64());
        Assert.Equal("apk", latest.GetProperty("PackageType").GetString());
        Assert.Equal(HttpStatusCode.NotFound, (await anonymous.GetAsync($"/api/store/v1/apps/{slug}/releases/latest?platform=android&arch=arm64&osVersion=25")).StatusCode);
    }

    [Fact]
    public async Task LatestRelease_UsesNumericPackageVersionsAndRejectsDowngrades()
    {
        var (appId, slug, releaseId) = await CreateAsync();
        Assert.Equal(HttpStatusCode.Created, (await SendUploadAsync(releaseId, Msix("1.0.9.0"), packageVersion: "1.0.9.0")).StatusCode);
        await PublishAsync(releaseId);
        var nextResponse = await _factory.Manager.PostAsJsonAsync($"/api/store/v1/admin/apps/{appId}/releases", new { Version = "1.0.1" });
        var nextId = (await nextResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Id").GetGuid();
        Assert.Equal(HttpStatusCode.Created, (await SendUploadAsync(nextId, Msix("1.0.10.0"), packageVersion: "1.0.10.0")).StatusCode);
        await PublishAsync(nextId);
        var olderResponse = await _factory.Manager.PostAsJsonAsync($"/api/store/v1/admin/apps/{appId}/releases", new { Version = "1.0.2" });
        var olderId = (await olderResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Id").GetGuid();
        Assert.Equal(HttpStatusCode.Created, (await SendUploadAsync(olderId, Msix("1.0.8.0"), packageVersion: "1.0.8.0")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await _factory.Manager.PostAsync($"/api/store/v1/admin/releases/{olderId}/publish", null)).StatusCode);
        using var anonymous = _factory.CreateClient();
        var latest = await anonymous.GetFromJsonAsync<JsonElement>($"/api/store/v1/apps/{slug}/releases/latest?platform=windows&arch=x64");
        Assert.Equal("1.0.10.0", latest.GetProperty("PackageVersion").GetString());
    }

    [Fact]
    public async Task MsixMetadata_MustMatchTheActualManifest()
    {
        var (_, _, releaseId) = await CreateAsync();
        using var response = await SendUploadAsync(releaseId, Msix("2.0.0.0"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<(Guid AppId, string Slug, Guid ReleaseId)> CreateAsync()
    {
        var slug = $"store-{Guid.NewGuid():N}";
        var created = await _factory.Manager.PostAsJsonAsync("/api/store/v1/admin/apps", new { Name = slug, Slug = slug });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var app = await created.Content.ReadFromJsonAsync<JsonElement>();
        var appId = app.GetProperty("Id").GetGuid();
        var response = await _factory.Manager.PostAsJsonAsync($"/api/store/v1/admin/apps/{appId}/releases", new { Version = "1.0.0" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var release = await response.Content.ReadFromJsonAsync<JsonElement>();
        return (appId, slug, release.GetProperty("Id").GetGuid());
    }

    private async Task<JsonElement> UploadAsync(Guid releaseId, byte[] bytes)
    {
        var response = await SendUploadAsync(releaseId, bytes);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private async Task PublishAsync(Guid releaseId)
    {
        var response = await _factory.Manager.PostAsync($"/api/store/v1/admin/releases/{releaseId}/publish", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private Task<HttpResponseMessage> SendUploadAsync(Guid releaseId, byte[] bytes, string? signature = null, string packageVersion = "1.0.0.0")
    {
        var request = new HttpRequestMessage(HttpMethod.Post,
            $"/api/store/v1/admin/releases/{releaseId}/artifacts?platform=windows&architecture=x64&packageType=msix&packageId=Arzenal.Test&packageVersion={packageVersion}&minimumOsVersion=10.0.19041.0");
        request.Content = new ByteArrayContent(bytes);
        request.Content.Headers.ContentType = new("application/octet-stream");
        request.Headers.Add("X-Publisher-Key-Id", "integration");
        request.Headers.Add("X-Artifact-Signature", signature ?? _factory.Sign(bytes));
        return _factory.Manager.SendAsync(request);
    }

    private static byte[] Msix(string packageVersion = "1.0.0.0")
    {
        using var output = new MemoryStream();
        using (var zip = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            // Archive inerte : elle teste le contrat serveur, pas une installation Windows.
            using (var writer = new StreamWriter(zip.CreateEntry("AppxManifest.xml").Open()))
                writer.Write($$"""
                    <Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10">
                      <Identity Name="Arzenal.Test" Publisher="CN=Integration" Version="{{packageVersion}}" ProcessorArchitecture="x64" />
                      <Dependencies><TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.19041.0" MaxVersionTested="10.0.22000.0" /></Dependencies>
                    </Package>
                    """);
            using (var writer = new StreamWriter(zip.CreateEntry("AppxBlockMap.xml").Open())) writer.Write("<BlockMap />");
            using (var writer = new StreamWriter(zip.CreateEntry("AppxSignature.p7x").Open())) writer.Write("inert fixture; not a native signature");
        }
        return output.ToArray();
    }
}
