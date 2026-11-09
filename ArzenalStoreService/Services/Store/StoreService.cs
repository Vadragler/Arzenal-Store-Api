using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.Service.Contracts.Store;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Interfaces;
using ArzenalStoreInfrastructure.Configurations;
using ArzenalStoreInfrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

namespace Arzenal.Store.Api.Service.Services.Store;

public sealed partial class StoreService(
    ApplicationDbContext db, IOptions<StorageSettings> storage,
    IOptions<StoreDistributionSettings> distribution, PublisherSignatureVerifier signatures,
    IFileScanner scanner)
{
    private IQueryable<AppArtifact> PublicArtifacts()
    {
        var keys = distribution.Value.PublisherPublicKeys.Where(key => !string.IsNullOrWhiteSpace(key.Value)).Select(key => key.Key).ToArray();
        return db.AppArtifacts.AsNoTracking().Where(artifact => artifact.Type == "app" &&
            artifact.ReleaseId != null && artifact.Release!.Status == "Published" &&
            artifact.Release.PublishedAt != null && artifact.App.IsVisible && artifact.App.Slug != null &&
            artifact.Sha256 != null && artifact.PackageType != null && artifact.Architecture != null &&
            artifact.PackageId != null && artifact.PackageVersion != null && artifact.MinimumOsVersion != null &&
            artifact.PublisherSignature != null && artifact.PublisherKeyId != null && keys.Contains(artifact.PublisherKeyId));
    }

    public async Task<StorePage<StoreAppDto>> CatalogAsync(StoreCatalogQuery query, CancellationToken ct)
    {
        StoreRules.ValidateQuery(query);
        var artifacts = PublicArtifacts().Where(artifact => artifact.Release!.Channel == query.Channel);
        if (query.Platform is not null)
            artifacts = artifacts.Where(artifact => artifact.Platform == query.Platform);
        if (query.Arch is not null)
            artifacts = artifacts.Where(artifact => artifact.Architecture == query.Arch ||
                artifact.Architecture == "neutral" || artifact.Architecture == "universal");
        // Les versions d'OS ne se comparent pas comme du texte. Seules les métadonnées sont chargées.
        List<Guid>? compatibleApps = null;
        if (query.OsVersion is not null)
        {
            var candidates = await artifacts.Select(artifact => new { artifact.AppId, artifact.Platform, artifact.MinimumOsVersion }).ToListAsync(ct);
            compatibleApps = candidates.Where(artifact => StoreRules.OsCompatible(artifact.Platform!, artifact.MinimumOsVersion!, query.OsVersion))
                .Select(artifact => artifact.AppId).Distinct().ToList();
        }
        var apps = db.Apps.AsNoTracking().Include(app => app.Category)
            .Where(app => artifacts.Any(artifact => artifact.AppId == app.Id));
        if (compatibleApps is not null)
            apps = apps.Where(app => compatibleApps.Contains(app.Id));
        if (query.CategoryId is not null)
            apps = apps.Where(app => app.CategoryId == query.CategoryId);
        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var search = query.Q.Trim();
            apps = apps.Where(app => app.Name.Contains(search) || (app.Description != null && app.Description.Contains(search)));
        }
        var count = await apps.CountAsync(ct);
        var items = await apps.OrderBy(app => app.Name).ThenBy(app => app.Id)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new(items.Select(MapApp).ToList(), query.Page, query.PageSize, count);
    }

    public async Task<StoreAppDto> PublicAppAsync(string slug, CancellationToken ct)
    {
        StoreRules.ValidateSlug(slug);
        var publicFiles = PublicArtifacts();
        var app = await db.Apps.AsNoTracking().Include(app => app.Category)
            .SingleOrDefaultAsync(app => app.Slug == slug && publicFiles.Any(artifact => artifact.AppId == app.Id), ct)
            ?? throw new NotFoundException("Application introuvable.");
        return MapApp(app);
    }

    public async Task<IReadOnlyList<StoreCategoryDto>> CategoriesAsync(CancellationToken ct)
    {
        var publicFiles = PublicArtifacts();
        return await db.Categories.AsNoTracking()
            .Where(category => publicFiles.Any(artifact => artifact.App.CategoryId == category.Id))
            .OrderBy(category => category.Name).ThenBy(category => category.Id)
            .Select(category => new StoreCategoryDto(category.Id, category.Name)).ToListAsync(ct);
    }

    public async Task<StoreArtifactDto> LatestAsync(string slug, StoreCatalogQuery query, CancellationToken ct)
    {
        StoreRules.ValidateQuery(query);
        if (query.Platform is null || query.Arch is null)
            throw new ValidationException("Plateforme et architecture requises.");
        var app = await PublicAppAsync(slug, ct);
        var candidates = await PublicArtifacts().Include(artifact => artifact.Release)
            .Where(artifact => artifact.AppId == app.Id && artifact.Platform == query.Platform && artifact.Release!.Channel == query.Channel)
            .ToListAsync(ct);
        var selected = candidates.Where(artifact => StoreRules.ArchitectureMatches(artifact.Architecture!, query.Arch) &&
                StoreRules.OsCompatible(artifact.Platform!, artifact.MinimumOsVersion!, query.OsVersion))
            .OrderByDescending(artifact => artifact.AndroidVersionCode ?? 0)
            .ThenByDescending(artifact => Version.Parse(artifact.PackageVersion!))
            .ThenByDescending(artifact => artifact.Architecture == query.Arch)
            .ThenByDescending(artifact => artifact.Release!.PublishedAt)
            .FirstOrDefault() ?? throw new NotFoundException("Aucune version compatible.");
        return MapArtifact(selected);
    }

    public async Task<StoreAppDto> SaveAppAsync(Guid? id, StoreAppRequest request, string actor, CancellationToken ct)
    {
        StoreRules.ValidateSlug(request.Slug);
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 255 || request.Description?.Length > 16000)
            throw new ValidationException("Fiche invalide.");
        if (request.CategoryId is not null && !await db.Categories.AnyAsync(category => category.Id == request.CategoryId, ct))
            throw new ValidationException("Catégorie inconnue.");
        var app = id is null ? new App { Name = request.Name.Trim(), Version = "0.0.0" } :
            await db.Apps.SingleOrDefaultAsync(app => app.Id == id, ct) ?? throw new NotFoundException("Application introuvable.");
        if (id is not null && app.Slug != request.Slug && await db.Releases.AnyAsync(release => release.AppId == id && release.PublishedAt != null, ct))
            throw new DuplicateException("Le slug d'une app déjà publiée est immuable.");
        if (await db.Apps.AnyAsync(other => other.Slug == request.Slug && other.Id != app.Id, ct))
            throw new DuplicateException("Slug déjà utilisé.");
        app.Name = request.Name.Trim();
        app.Slug = request.Slug;
        app.Description = request.Description;
        app.CategoryId = request.CategoryId;
        app.IsVisible = request.IsVisible;
        if (id is null) db.Apps.Add(app);
        Audit(app.Id, null, null, actor, id is null ? "app-created" : "app-updated");
        await db.SaveChangesAsync(ct);
        return MapApp(app);
    }

    public async Task<StoreReleaseDto> CreateReleaseAsync(Guid appId, StoreReleaseRequest request, string actor, CancellationToken ct)
    {
        if (!Version.TryParse(request.Version, out _) || request.Version.Length > 50 || request.Channel != "stable" || request.Changelog?.Length > 16000)
            throw new ValidationException("Version ou canal invalide.");
        var app = await db.Apps.SingleOrDefaultAsync(app => app.Id == appId, ct) ?? throw new NotFoundException("Application introuvable.");
        if (app.Slug is null) throw new DuplicateException("Configurer d'abord le slug de cette application via le Store V1.");
        if (await db.Releases.AnyAsync(release => release.AppId == appId && release.Version == request.Version, ct))
            throw new DuplicateException("Version déjà existante.");
        var release = new Release { AppId = appId, Version = request.Version, Status = "Draft", Channel = request.Channel, Changelog = request.Changelog };
        db.Releases.Add(release);
        Audit(appId, release.Id, null, actor, "release-created");
        await db.SaveChangesAsync(ct);
        return MapRelease(release);
    }

    public async Task<StoreReleaseDto> ReleaseAsync(Guid releaseId, CancellationToken ct) => MapRelease(
        await db.Releases.AsNoTracking().Include(release => release.Artifacts).SingleOrDefaultAsync(release => release.Id == releaseId, ct)
        ?? throw new NotFoundException("Release introuvable."));

    public async Task<IReadOnlyList<StoreReleaseDto>> ReleasesAsync(Guid appId, CancellationToken ct)
    {
        if (!await db.Apps.AnyAsync(app => app.Id == appId, ct)) throw new NotFoundException("Application introuvable.");
        var releases = await db.Releases.AsNoTracking().Include(release => release.Artifacts)
            .Where(release => release.AppId == appId).OrderByDescending(release => release.CreatedAt).ToListAsync(ct);
        return releases.Select(MapRelease).ToList();
    }

    public async Task<IReadOnlyList<StoreAuditDto>> AuditAsync(Guid appId, CancellationToken ct) =>
        await db.PublicationAudits.AsNoTracking().Where(audit => audit.AppId == appId)
            .OrderByDescending(audit => audit.OccurredAt).ThenBy(audit => audit.Id).Take(500)
            .Select(audit => new StoreAuditDto(audit.Id, audit.ReleaseId, audit.ArtifactId, audit.ActorId, audit.Action, audit.OccurredAt)).ToListAsync(ct);

    private void Audit(Guid appId, Guid? releaseId, Guid? artifactId, string actor, string action)
    {
        if (string.IsNullOrWhiteSpace(actor) || actor.Length > 200) throw new UnauthorizedAccessException();
        db.PublicationAudits.Add(new PublicationAudit { AppId = appId, ReleaseId = releaseId, ArtifactId = artifactId, ActorId = actor, Action = action });
    }

    private StoreAppDto MapApp(App app) => new(app.Id, app.Slug!, app.Name, app.Description,
        distribution.Value.PublisherDisplayName, app.CategoryId, app.Category?.Name, app.IsVisible, $"/api/store/v1/apps/{app.Slug}/icon");

    private static StoreArtifactDto MapArtifact(AppArtifact artifact) => new(artifact.Id, artifact.AppId,
        artifact.ReleaseId!.Value, artifact.Version ?? "", artifact.Platform ?? "", artifact.Architecture ?? "",
        artifact.PackageType ?? "", artifact.PackageId ?? "", artifact.PackageVersion ?? "", artifact.MinimumOsVersion ?? "",
        artifact.AndroidVersionCode, artifact.FileSize, artifact.Sha256 ?? "",
        new("rsa-pss-sha256", artifact.PublisherKeyId ?? "", artifact.PublisherSignature ?? ""),
        $"/api/store/v1/artifacts/{artifact.Id}/download");

    private static StoreReleaseDto MapRelease(Release release) => new(release.Id, release.AppId, release.Version,
        release.Channel, release.Status, release.Changelog, release.CreatedAt, release.PublishedAt,
        release.Artifacts.Select(MapArtifact).ToList());

    private async Task<IDbContextTransaction?> BeginTransactionAsync(CancellationToken ct) =>
        db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, ct) : null;

    private async Task<Release> LockReleaseAsync(Guid id, CancellationToken ct)
    {
        if (!db.Database.IsRelational())
            return await db.Releases.SingleOrDefaultAsync(release => release.Id == id, ct) ?? throw new NotFoundException("Release introuvable.");
        var appId = await db.Releases.AsNoTracking().Where(release => release.Id == id).Select(release => (Guid?)release.AppId).SingleOrDefaultAsync(ct)
            ?? throw new NotFoundException("Release introuvable.");
        // L'app puis la release : un ordre de verrouillage commun sérialise aussi deux publications concurrentes.
        await db.Apps.FromSqlInterpolated($"SELECT * FROM Apps WHERE Id = {appId} FOR UPDATE").ToListAsync(ct);
        var releases = await db.Releases.FromSqlInterpolated($"SELECT * FROM Releases WHERE Id = {id} FOR UPDATE").ToListAsync(ct);
        return releases.SingleOrDefault() ?? throw new NotFoundException("Release introuvable.");
    }
}
