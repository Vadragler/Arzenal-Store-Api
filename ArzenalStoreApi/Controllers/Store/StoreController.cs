using Arzenal.Store.Api.Service.Contracts.Store;
using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Store.Api.Service.Services.Store;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Net.Http.Headers;

namespace ArzenalStoreApi.Controllers.Store;

[ApiController]
[Route("api/store/v1")]
[AllowAnonymous]
[EnableRateLimiting("store-read")]
public sealed class StoreController(StoreService store, IAppFilesService files) : ControllerBase
{
    [HttpGet("apps")]
    [HttpGet("apps/search")]
    [ResponseCache(Duration = 30)]
    public async Task<ActionResult<StorePage<StoreAppDto>>> Catalog([FromQuery] StoreCatalogQuery query, CancellationToken ct) =>
        Ok(await store.CatalogAsync(query, ct));

    [HttpGet("apps/{slug}")]
    [ResponseCache(Duration = 30)]
    public async Task<ActionResult<StoreAppDto>> App(string slug, CancellationToken ct) => Ok(await store.PublicAppAsync(slug, ct));

    [HttpGet("categories")]
    [ResponseCache(Duration = 60)]
    public async Task<ActionResult<IReadOnlyList<StoreCategoryDto>>> Categories(CancellationToken ct) => Ok(await store.CategoriesAsync(ct));

    [HttpGet("apps/{slug}/releases/latest")]
    [ResponseCache(Duration = 15)]
    public async Task<ActionResult<StoreArtifactDto>> Latest(string slug, [FromQuery] StoreCatalogQuery query, CancellationToken ct) =>
        Ok(await store.LatestAsync(slug, query, ct));

    [HttpGet("apps/{slug}/icon")]
    public async Task<IActionResult> Icon(string slug, CancellationToken ct)
    {
        var app = await store.PublicAppAsync(slug, ct);
        var icon = await files.GetAppIcon(app.Id);
        return File(icon.Stream, icon.ContentType, icon.FileName, enableRangeProcessing: true);
    }

    [HttpGet("artifacts/{artifactId:guid}/download")]
    [EnableRateLimiting("store-download")]
    public async Task<IActionResult> Download(Guid artifactId, CancellationToken ct)
    {
        var result = await store.DownloadAsync(artifactId, ct);
        // Le fichier est immuable mais son accès peut être retiré : le client doit revalider.
        Response.Headers.CacheControl = "public, max-age=0, must-revalidate";
        return File(result.Stream, "application/octet-stream",
            $"{result.Artifact.PackageId}-{result.Artifact.PackageVersion}.{result.Artifact.PackageType}",
            new DateTimeOffset(DateTime.SpecifyKind(result.UploadedAt, DateTimeKind.Utc)),
            new EntityTagHeaderValue($"\"{result.Artifact.Sha256}\""), enableRangeProcessing: true);
    }
}
