using System.ComponentModel.DataAnnotations;

namespace Arzenal.Store.Api.Service.Contracts.Store;

public sealed class StoreAppRequest
{
    [Required, StringLength(255, MinimumLength = 1)] public string Name { get; set; } = "";
    [Required, StringLength(160, MinimumLength = 1)] public string Slug { get; set; } = "";
    [StringLength(16000)] public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public bool IsVisible { get; set; } = true;
}

public sealed class StoreReleaseRequest
{
    [Required, StringLength(50)] public string Version { get; set; } = "";
    [Required] public string Channel { get; set; } = "stable";
    [StringLength(16000)] public string? Changelog { get; set; }
}

public sealed class StoreCatalogQuery
{
    [Range(1, int.MaxValue)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 20;
    [StringLength(200)] public string? Q { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Platform { get; set; }
    public string? Arch { get; set; }
    public string? OsVersion { get; set; }
    public string Channel { get; set; } = "stable";
}

public sealed class StoreArtifactRequest
{
    [Required] public string Platform { get; set; } = "";
    [Required] public string Architecture { get; set; } = "";
    [Required] public string PackageType { get; set; } = "";
    [Required, StringLength(255)] public string PackageId { get; set; } = "";
    [Required, StringLength(50)] public string PackageVersion { get; set; } = "";
    [Required, StringLength(50)] public string MinimumOsVersion { get; set; } = "";
    public long? AndroidVersionCode { get; set; }
}

public sealed record StorePage<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public sealed record StoreAppDto(Guid Id, string Slug, string Name, string? Description,
    string Publisher, Guid? CategoryId, string? Category, bool IsVisible, string IconEndpoint);

public sealed record StoreSignatureDto(string Type, string KeyId, string Value);

public sealed record StoreArtifactDto(Guid Id, Guid ApplicationId, Guid ReleaseId, string VersionName,
    string Platform, string Architecture, string PackageType, string PackageId, string PackageVersion,
    string MinimumOsVersion, long? AndroidVersionCode, long SizeBytes, string Sha256,
    StoreSignatureDto Signature, string DownloadEndpoint);

public sealed record StoreReleaseDto(Guid Id, Guid ApplicationId, string Version, string Channel,
    string Status, string? Changelog, DateTime CreatedAt, DateTime? PublishedAt,
    IReadOnlyList<StoreArtifactDto> Artifacts);

public sealed record StoreCategoryDto(Guid Id, string Name);
public sealed record StoreAuditDto(Guid Id, Guid? ReleaseId, Guid? ArtifactId, string ActorId, string Action, DateTime OccurredAt);
