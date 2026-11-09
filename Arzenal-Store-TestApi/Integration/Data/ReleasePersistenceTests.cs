using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.TestIntegration.Infrastructure;
using ArzenalStoreInfrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;

namespace Arzenal.Store.Api.TestIntegration.Integration.Data;

public class ReleasePersistenceTests : IsolatedApiTest
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public ReleasePersistenceTests()
    {
        _factory = Factory;
    }

    [Fact]
    public async Task Database_AppliesReleaseMigrationAndMatchesTheCurrentModel()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var appliedMigrations = await db.Database.GetAppliedMigrationsAsync();

        Assert.Contains(appliedMigrations, migration => migration.EndsWith("_AddReleases", StringComparison.Ordinal));
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        Assert.False(db.Database.HasPendingModelChanges());
    }

    [Fact]
    public async Task SaveDraft_PersistsItsAppAndAllowsNoPublicationDate()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var app = NewApp();
        var beforeCreation = DateTime.UtcNow;
        var release = NewRelease(app.Id);
        var afterCreation = DateTime.UtcNow;
        db.Apps.Add(app);
        db.Releases.Add(release);
        await db.SaveChangesAsync();

        var saved = await db.Releases.AsNoTracking().Include(item => item.App)
            .SingleAsync(item => item.Id == release.Id);

        Assert.NotEqual(Guid.Empty, saved.Id);
        Assert.Equal(app.Id, saved.AppId);
        Assert.Equal(app.Name, saved.App.Name);
        Assert.Equal("1.0.0", saved.Version);
        Assert.Equal("Draft", saved.Status);
        Assert.Null(saved.PublishedAt);
        // MySQL conserve six décimales, contre sept pour DateTime.
        Assert.InRange(saved.CreatedAt, beforeCreation.AddTicks(-10), afterCreation.AddTicks(10));
    }

    [Fact]
    public async Task SavePublishedRelease_PreservesDatesToTheMicrosecond()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var app = NewApp();
        var createdAt = new DateTime(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc).AddTicks(1_234_560);
        var publishedAt = createdAt.AddDays(1).AddTicks(10);
        var release = NewRelease(app.Id);
        release.Status = "Published";
        release.CreatedAt = createdAt;
        release.PublishedAt = publishedAt;
        db.Apps.Add(app);
        db.Releases.Add(release);
        await db.SaveChangesAsync();

        var saved = await db.Releases.AsNoTracking().SingleAsync(item => item.Id == release.Id);

        Assert.Equal("Published", saved.Status);
        Assert.Equal(createdAt, saved.CreatedAt);
        Assert.Equal(publishedAt, saved.PublishedAt);
    }

    [Fact]
    public async Task LoadApp_IncludesItsMultipleReleases()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var app = NewApp();
        app.Releases.Add(NewRelease(app.Id, "1.0.0"));
        app.Releases.Add(NewRelease(app.Id, "1.1.0"));
        db.Apps.Add(app);
        await db.SaveChangesAsync();

        var saved = await db.Apps.AsNoTracking().Include(item => item.Releases)
            .SingleAsync(item => item.Id == app.Id);

        Assert.Equal(new[] { "1.0.0", "1.1.0" }, saved.Releases.Select(item => item.Version).OrderBy(version => version));
        Assert.All(saved.Releases, release => Assert.Equal(app.Id, release.AppId));
    }

    [Fact]
    public async Task SaveRelease_RejectsDuplicateVersionForTheSameApp()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var app = NewApp();
        db.Apps.Add(app);
        db.Releases.Add(NewRelease(app.Id));
        await db.SaveChangesAsync();
        db.Releases.Add(NewRelease(app.Id));

        await AssertMySqlFailureAsync(db, 1062); // ER_DUP_ENTRY
    }

    [Fact]
    public async Task SaveRelease_RejectsChangingVersionToAnExistingVersion()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var app = NewApp();
        var second = NewRelease(app.Id, "1.1.0");
        db.Apps.Add(app);
        db.Releases.AddRange(NewRelease(app.Id), second);
        await db.SaveChangesAsync();
        second.Version = "1.0.0";

        await AssertMySqlFailureAsync(db, 1062); // ER_DUP_ENTRY
    }

    [Fact]
    public async Task SaveRelease_AllowsTheSameVersionForDifferentApps()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var firstApp = NewApp();
        var secondApp = NewApp();
        db.Apps.AddRange(firstApp, secondApp);
        db.Releases.AddRange(NewRelease(firstApp.Id), NewRelease(secondApp.Id));

        await db.SaveChangesAsync();

        Assert.Equal(2, await db.Releases.CountAsync(release =>
            release.AppId == firstApp.Id || release.AppId == secondApp.Id));
    }

    [Theory]
    [InlineData(nameof(Release.Version))]
    [InlineData(nameof(Release.Status))]
    public async Task SaveRelease_RejectsNullRequiredText(string propertyName)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var app = NewApp();
        db.Apps.Add(app);
        await db.SaveChangesAsync();
        var release = NewRelease(app.Id);
        db.Releases.Add(release);
        db.Entry(release).Property(propertyName).CurrentValue = null;

        await AssertMySqlFailureAsync(db, 1048); // ER_BAD_NULL_ERROR
    }

    [Theory]
    [InlineData(nameof(Release.Version), 51)]
    [InlineData(nameof(Release.Status), 21)]
    public async Task SaveRelease_RejectsTextAboveTheConfiguredLimit(string propertyName, int length)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var app = NewApp();
        db.Apps.Add(app);
        await db.SaveChangesAsync();
        var release = NewRelease(app.Id);
        db.Releases.Add(release);
        db.Entry(release).Property(propertyName).CurrentValue = new string('x', length);

        await AssertMySqlFailureAsync(db, 1406); // ER_DATA_TOO_LONG
    }

    [Fact]
    public async Task SaveRelease_AllowsTextAtTheConfiguredLimits()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var app = NewApp();
        var release = NewRelease(app.Id, new string('v', 50));
        release.Status = new string('s', 20);
        db.Apps.Add(app);
        db.Releases.Add(release);

        await db.SaveChangesAsync();

        var saved = await db.Releases.AsNoTracking().SingleAsync(item => item.Id == release.Id);
        Assert.Equal(release.Version, saved.Version);
        Assert.Equal(release.Status, saved.Status);
    }

    [Fact]
    public async Task SaveRelease_RejectsAnAppThatDoesNotExist()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Releases.Add(NewRelease(Guid.NewGuid()));

        await AssertMySqlFailureAsync(db, 1452); // ER_NO_REFERENCED_ROW_2
    }

    [Fact]
    public async Task DeleteApp_DeletesOnlyItsReleasesInMySql()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var firstApp = NewApp();
        var secondApp = NewApp();
        db.Apps.AddRange(firstApp, secondApp);
        db.Releases.AddRange(
            NewRelease(firstApp.Id), NewRelease(firstApp.Id, "1.1.0"), NewRelease(secondApp.Id));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        // La suppression directe oblige MySQL, et non le suivi EF, à supprimer les enfants.
        var deletedApps = await db.Apps.Where(app => app.Id == firstApp.Id).ExecuteDeleteAsync();

        Assert.Equal(1, deletedApps);
        Assert.False(await db.Releases.AnyAsync(release => release.AppId == firstApp.Id));
        Assert.True(await db.Apps.AnyAsync(app => app.Id == secondApp.Id));
        Assert.Equal(1, await db.Releases.CountAsync(release => release.AppId == secondApp.Id));
    }

    [Fact]
    public async Task DeleteRelease_KeepsItsAppAndOtherReleases()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var app = NewApp();
        var first = NewRelease(app.Id);
        var second = NewRelease(app.Id, "1.1.0");
        db.Apps.Add(app);
        db.Releases.AddRange(first, second);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await db.Releases.Where(release => release.Id == first.Id).ExecuteDeleteAsync();

        Assert.True(await db.Apps.AnyAsync(item => item.Id == app.Id));
        var remaining = await db.Releases.AsNoTracking().SingleAsync(release => release.AppId == app.Id);
        Assert.Equal(second.Id, remaining.Id);
    }

    private static App NewApp() => new()
    {
        Name = $"Release test {Guid.NewGuid():N}",
        Version = "1.0.0"
    };

    [Fact]
    public async Task Artifact_CannotReferenceAReleaseOfAnotherApp()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var owner = NewApp();
        var other = NewApp();
        var release = NewRelease(owner.Id);
        db.Apps.AddRange(owner, other);
        db.Releases.Add(release);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        db.AppArtifacts.Add(new AppArtifact
        {
            AppId = other.Id,
            ReleaseId = release.Id,
            Type = "app",
            Locator = Guid.NewGuid().ToString("N"),
            RelativePath = "not-written.msix",
            OriginalFileName = "not-written.msix",
            ContentType = "application/octet-stream"
        });

        await AssertMySqlFailureAsync(db, 1452);
    }

    private static Release NewRelease(Guid appId, string version = "1.0.0") => new()
    {
        AppId = appId,
        Version = version,
        Status = "Draft"
    };

    private static async Task AssertMySqlFailureAsync(ApplicationDbContext db, int expectedErrorNumber)
    {
        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        var mysqlException = Assert.IsType<MySqlException>(exception.InnerException);
        Assert.Equal(expectedErrorNumber, mysqlException.Number);
    }
}
