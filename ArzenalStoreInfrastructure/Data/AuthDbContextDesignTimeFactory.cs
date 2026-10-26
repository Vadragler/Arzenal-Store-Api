using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Arzenal.Store.Api.Infrastructure.Data;

/// <summary>
/// Factory isolée pour les commandes EF Core de la base d'authentification.
/// </summary>
public sealed class AuthDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__AuthConnection")
            ?? "Server=localhost;Port=3306;Database=arzenal_auth_design;User Id=design;Password=design;";
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 33)))
            .Options;

        return new AuthDbContext(options);
    }
}
