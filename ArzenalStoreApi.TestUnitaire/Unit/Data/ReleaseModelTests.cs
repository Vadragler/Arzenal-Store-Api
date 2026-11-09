using Arzenal.Store.Api.Domain.Models;
using ArzenalStoreInfrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Data;

public class ReleaseModelTests
{
    [Fact]
    public void Model_MapsReleasesTableWithIdAsPrimaryKey()
    {
        using var db = CreateContext();
        var entity = db.Model.FindEntityType(typeof(Release));

        Assert.NotNull(entity);
        Assert.Equal("Releases", entity.GetTableName());
        var primaryKey = entity.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Equal(nameof(Release.Id), Assert.Single(primaryKey.Properties).Name);
    }

    [Theory]
    [InlineData(nameof(Release.Version), 50)]
    [InlineData(nameof(Release.Status), 20)]
    public void Model_RequiresReleaseTextAndLimitsItsLength(string propertyName, int maximumLength)
    {
        using var db = CreateContext();
        var property = db.Model.FindEntityType(typeof(Release))!.FindProperty(propertyName);

        Assert.NotNull(property);
        Assert.False(property.IsNullable);
        Assert.Equal(maximumLength, property.GetMaxLength());
    }

    [Theory]
    [InlineData(nameof(Release.CreatedAt), false)]
    [InlineData(nameof(Release.PublishedAt), true)]
    public void Model_StoresReleaseDatesWithMicrosecondsAndExpectedNullability(
        string propertyName, bool isNullable)
    {
        using var db = CreateContext();
        var property = db.Model.FindEntityType(typeof(Release))!.FindProperty(propertyName);

        Assert.NotNull(property);
        Assert.Equal(isNullable, property.IsNullable);
        Assert.Equal("datetime(6)", property.GetColumnType());
    }

    [Fact]
    public void Model_RequiresAnAppAndConfiguresCascadeDelete()
    {
        using var db = CreateContext();
        var entity = db.Model.FindEntityType(typeof(Release))!;
        var relationship = Assert.Single(entity.GetForeignKeys());

        Assert.Equal(typeof(App), relationship.PrincipalEntityType.ClrType);
        Assert.Equal(nameof(Release.AppId), Assert.Single(relationship.Properties).Name);
        Assert.Equal(nameof(Release.App), relationship.DependentToPrincipal!.Name);
        Assert.Equal(nameof(App.Releases), relationship.PrincipalToDependent!.Name);
        Assert.True(relationship.IsRequired);
        Assert.False(relationship.IsUnique);
        Assert.Equal(DeleteBehavior.Cascade, relationship.DeleteBehavior);
    }

    [Fact]
    public void Model_MakesVersionUniqueWithinEachApp()
    {
        using var db = CreateContext();
        var entity = db.Model.FindEntityType(typeof(Release))!;
        var index = Assert.Single(entity.GetIndexes(), index => index.Properties
            .Select(property => property.Name)
            .SequenceEqual(new[] { nameof(Release.AppId), nameof(Release.Version) }));

        Assert.True(index.IsUnique);
    }

    private static ApplicationDbContext CreateContext()
    {
        // La lecture du modèle utilise le fournisseur MySQL sans ouvrir de connexion.
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql(
                "Server=localhost;Database=release_model_tests;User Id=unused;Password=unused;",
                new MySqlServerVersion(new Version(8, 0, 33)))
            .Options;

        return new ApplicationDbContext(options);
    }
}
