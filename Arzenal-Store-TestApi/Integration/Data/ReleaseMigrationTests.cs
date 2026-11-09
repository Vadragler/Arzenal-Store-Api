using Arzenal.Store.Api.Domain.Models;
using ArzenalStoreInfrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Arzenal.Store.Api.TestIntegration.Infrastructure;

namespace Arzenal.Store.Api.TestIntegration.Integration.Data;

public class ReleaseMigrationTests
{
    [Fact]
    public async Task AddReleases_CanUpgradeRollbackAndReapplyWithoutLosingApps()
    {
        // Le rollback utilise sa propre base, sur le serveur partagé de cette exécution.
        await using var database = await MySqlTestDatabase.CreateAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql(database.StoreConnectionString, new MySqlServerVersion(new Version(8, 0, 33)))
            .Options;
        await using var db = new ApplicationDbContext(options);
        var migrator = db.GetService<IMigrator>();
        const string previousMigration = "20261026183240_AddAppArtifactMetadata";
        await migrator.MigrateAsync(previousMigration);
        var app = new App { Name = "Existing app", Version = "1.0.0" };
        // Le schéma historique ne contient pas encore les colonnes du modèle courant.
        await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO Apps (Id, Name, Version, IsVisible, AppSize) VALUES ({app.Id}, {app.Name}, {app.Version}, {true}, {0L});");
        var legacyAppId = Guid.NewGuid();
        var legacyArtifactId = Guid.NewGuid();
        var uploadedAt = DateTime.UtcNow;
        await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO Apps (Id, Name, Version, IsVisible, AppSize) VALUES ({legacyAppId}, {"Legacy app"}, {"2.0.0"}, {true}, {10L});");
        await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO AppArtifacts (Id, AppId, Type, Locator, Version, Platform, OriginalFileName, RelativePath, ContentType, FileSize, UploadedAt) VALUES ({legacyArtifactId}, {legacyAppId}, {"app"}, {"2.0.0|windows"}, {"2.0.0"}, {"windows"}, {"legacy.exe"}, {"legacy/legacy.exe"}, {"application/octet-stream"}, {10L}, {uploadedAt});");

        await migrator.MigrateAsync();
        var legacy = await db.AppArtifacts.AsNoTracking().Include(item => item.Release).SingleAsync(item => item.Id == legacyArtifactId);
        Assert.NotNull(legacy.ReleaseId);
        Assert.Equal("Draft", legacy.Release!.Status);
        Assert.Equal("stable", legacy.Release.Channel);
        Assert.Null(legacy.Release.PublishedAt);
        Assert.Null(legacy.Sha256);
        Assert.Equal("legacy/legacy.exe", legacy.RelativePath);
        db.Releases.Add(new Release { AppId = app.Id, Version = "1.0.0", Status = "Draft" });
        await db.SaveChangesAsync();
        Assert.Equal(1, await db.Releases.CountAsync(item => item.AppId == app.Id));
        db.ChangeTracker.Clear();

        await migrator.MigrateAsync(previousMigration);

        Assert.False(await ReleasesTableExistsAsync(db));
        Assert.Equal(app.Name, await db.Apps.AsNoTracking().Where(item => item.Id == app.Id).Select(item => item.Name).SingleAsync());

        await migrator.MigrateAsync();

        Assert.True(await ReleasesTableExistsAsync(db));
        Assert.Empty(await db.Releases.AsNoTracking().Where(item => item.AppId == app.Id).ToListAsync());
        db.Releases.Add(new Release { AppId = app.Id, Version = "1.1.0", Status = "Draft" });
        await db.SaveChangesAsync();
        Assert.Equal("1.1.0", (await db.Releases.AsNoTracking().SingleAsync(item => item.AppId == app.Id)).Version);
        Assert.True(await db.Apps.AnyAsync(item => item.Id == app.Id));
    }

    private static async Task<bool> ReleasesTableExistsAsync(ApplicationDbContext db)
    {
        await db.Database.OpenConnectionAsync();
        try
        {
            await using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = """
                SELECT COUNT(*) FROM information_schema.tables
                WHERE table_schema = DATABASE() AND table_name = 'Releases';
                """;
            return Convert.ToInt64(await command.ExecuteScalarAsync()) == 1;
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }
}
