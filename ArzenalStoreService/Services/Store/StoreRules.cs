using System.Globalization;
using System.Text.RegularExpressions;
using Arzenal.Store.Api.Service.Contracts.Store;
using Arzenal.Store.Api.Service.Exceptions;

namespace Arzenal.Store.Api.Service.Services.Store;

public static class StoreRules
{
    public static void ValidateSlug(string slug)
    {
        if (string.IsNullOrEmpty(slug) || slug.Length > 160 ||
            !Regex.IsMatch(slug, "^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)))
            throw new ValidationException("Le slug doit contenir des lettres minuscules, chiffres et tirets.");
    }

    public static void ValidateQuery(StoreCatalogQuery query)
    {
        if (query.Page < 1 || query.PageSize is < 1 or > 100 || (long)(query.Page - 1) * query.PageSize > int.MaxValue ||
            query.Q?.Length > 200 || query.Channel != "stable")
            throw new ValidationException("Pagination, recherche ou canal invalide.");
        if (query.Platform is not null && query.Platform is not ("windows" or "android"))
            throw new ValidationException("Plateforme non prise en charge.");
        if (query.Arch is not null)
        {
            if (query.Platform is null || !ArchitectureAllowed(query.Platform, query.Arch))
                throw new ValidationException("Architecture non prise en charge.");
        }
        if (query.OsVersion is not null)
        {
            if (query.Platform is null || !OsVersionValid(query.Platform, query.OsVersion))
                throw new ValidationException("Version du système invalide.");
        }
    }

    public static void ValidateArtifact(StoreArtifactRequest request)
    {
        if (!ArchitectureAllowed(request.Platform, request.Architecture) ||
            (request.Platform, request.PackageType) is not (("windows", "msix") or ("android", "apk")) ||
            string.IsNullOrWhiteSpace(request.PackageId) || request.PackageId.Length > 255 ||
            !Regex.IsMatch(request.PackageId, "^[A-Za-z0-9][A-Za-z0-9._-]+$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)) ||
            !Version.TryParse(request.PackageVersion, out _) ||
            !OsVersionValid(request.Platform, request.MinimumOsVersion) ||
            (request.Platform == "android" && request.AndroidVersionCode is not > 0) ||
            (request.Platform == "windows" && request.AndroidVersionCode is not null))
            throw new ValidationException("Métadonnées du paquet invalides.");
    }

    public static bool ArchitectureAllowed(string platform, string arch) => platform switch
    {
        "windows" => arch is "x64" or "x86" or "arm64" or "neutral",
        "android" => arch is "arm64" or "arm" or "x64" or "x86" or "universal",
        _ => false
    };

    public static bool ArchitectureMatches(string artifact, string? device) =>
        device is null || artifact == device || artifact is "neutral" or "universal";

    private static bool OsVersionValid(string platform, string version) =>
        platform == "android"
            ? int.TryParse(version, NumberStyles.None, CultureInfo.InvariantCulture, out var level) && level > 0
            : version.Length <= 50 && Version.TryParse(version, out _);

    public static bool OsCompatible(string platform, string minimum, string? device) => device is null ||
        (platform == "android"
            ? int.Parse(device, CultureInfo.InvariantCulture) >= int.Parse(minimum, CultureInfo.InvariantCulture)
            : Version.Parse(device) >= Version.Parse(minimum));
}
