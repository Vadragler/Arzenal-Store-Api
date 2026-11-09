using Arzenal.Store.Api.Service.Contracts.Store;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Services.Store;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Services;

public class StoreRulesTests
{
    [Theory]
    [InlineData("windows", "x64", "10.0.19041.0", "10.0.22000.0", true)]
    [InlineData("windows", "neutral", "10.0.22000.0", "10.0.19041.0", false)]
    [InlineData("android", "arm64", "26", "35", true)]
    [InlineData("android", "universal", "35", "26", false)]
    public void Compatibility_UsesNumericOsVersions(string platform, string architecture, string minimum, string device, bool expected)
    {
        Assert.Equal(expected, StoreRules.OsCompatible(platform, minimum, device));
        StoreRules.ValidateQuery(new StoreCatalogQuery { Platform = platform, Arch = architecture, OsVersion = device });
    }

    [Theory]
    [InlineData("ios", "arm64", null)]
    [InlineData("windows", "unknown", null)]
    [InlineData(null, "arm64", null)]
    [InlineData(null, null, "35")]
    [InlineData("android", "arm64", "8.1")]
    [InlineData("windows", "x64", "banana")]
    public void Query_RejectsUnsupportedOrAmbiguousCompatibility(string? platform, string? arch, string? os)
    {
        Assert.Throws<ValidationException>(() => StoreRules.ValidateQuery(new StoreCatalogQuery { Platform = platform, Arch = arch, OsVersion = os }));
    }

    [Theory]
    [InlineData("../app")]
    [InlineData("Mon App")]
    [InlineData("app/")]
    [InlineData("")]
    public void Slug_RejectsUnsafeOrNonCanonicalValues(string slug) => Assert.Throws<ValidationException>(() => StoreRules.ValidateSlug(slug));
}
