using System.Security.Claims;
using Arzenal.Store.Api.Service.Contracts.Store;
using Arzenal.Store.Api.Service.Services.Store;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ArzenalStoreApi.Controllers.Store;

[ApiController]
[Route("api/store/v1/admin")]
[Authorize(Policy = "ArzenalStoreManager")]
public sealed class StoreAdminController(StoreService store) : ControllerBase
{
    private string Actor => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();

    [HttpPost("apps")]
    public async Task<ActionResult<StoreAppDto>> CreateApp(StoreAppRequest request, CancellationToken ct)
    {
        var result = await store.SaveAppAsync(null, request, Actor, ct);
        return Created($"/api/store/v1/admin/apps/{result.Id}/releases", result);
    }

    [HttpPut("apps/{appId:guid}")]
    public async Task<ActionResult<StoreAppDto>> UpdateApp(Guid appId, StoreAppRequest request, CancellationToken ct) =>
        Ok(await store.SaveAppAsync(appId, request, Actor, ct));

    [HttpPost("apps/{appId:guid}/releases")]
    public async Task<ActionResult<StoreReleaseDto>> CreateRelease(Guid appId, StoreReleaseRequest request, CancellationToken ct)
    {
        var result = await store.CreateReleaseAsync(appId, request, Actor, ct);
        return CreatedAtAction(nameof(Release), new { releaseId = result.Id }, result);
    }

    [HttpGet("apps/{appId:guid}/releases")]
    public async Task<ActionResult<IReadOnlyList<StoreReleaseDto>>> Releases(Guid appId, CancellationToken ct) => Ok(await store.ReleasesAsync(appId, ct));

    [HttpGet("apps/{appId:guid}/audit")]
    public async Task<ActionResult<IReadOnlyList<StoreAuditDto>>> Audit(Guid appId, CancellationToken ct) => Ok(await store.AuditAsync(appId, ct));

    [HttpGet("releases/{releaseId:guid}")]
    public async Task<ActionResult<StoreReleaseDto>> Release(Guid releaseId, CancellationToken ct) => Ok(await store.ReleaseAsync(releaseId, ct));

    [HttpPost("releases/{releaseId:guid}/artifacts")]
    [Consumes("application/octet-stream")]
    [RequestSizeLimit(268435456)]
    [EnableRateLimiting("upload")]
    public async Task<ActionResult<StoreArtifactDto>> Upload(Guid releaseId, [FromQuery] StoreArtifactRequest request,
        [FromHeader(Name = "X-Publisher-Key-Id")] string keyId,
        [FromHeader(Name = "X-Artifact-Signature")] string signature, CancellationToken ct)
    {
        var result = await store.UploadAsync(releaseId, request, Request.Body, keyId, signature, Actor, ct);
        return Created($"/api/store/v1/admin/releases/{releaseId}", result);
    }

    [HttpDelete("releases/{releaseId:guid}/artifacts/{artifactId:guid}")]
    public async Task<IActionResult> DeleteArtifact(Guid releaseId, Guid artifactId, CancellationToken ct)
    {
        await store.DeleteArtifactAsync(releaseId, artifactId, Actor, ct);
        return NoContent();
    }

    [HttpPost("releases/{releaseId:guid}/publish")]
    public async Task<ActionResult<StoreReleaseDto>> Publish(Guid releaseId, CancellationToken ct) => Ok(await store.PublishAsync(releaseId, Actor, ct));

    [HttpPost("releases/{releaseId:guid}/withdraw")]
    public async Task<ActionResult<StoreReleaseDto>> Withdraw(Guid releaseId, CancellationToken ct) => Ok(await store.WithdrawAsync(releaseId, Actor, ct));
}
