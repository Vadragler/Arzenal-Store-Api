using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ArzenalStoreInfrastructure.Data;

/// <summary>
/// Permet à EF Core de générer et vérifier les migrations sans démarrer l'API
/// ni nécessiter les secrets locaux de développement.
/// </summary>
public sealed class ApplicationDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__StoreConnection")
            ?? "Server=localhost;Port=3306;Database=arzenal_design;User Id=design;Password=design;";
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 33)))
            .Options;

        return new ApplicationDbContext(options);
    }
}
