using System.Net;
using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.TestIntegration.Infrastructure;
using ArzenalStoreInfrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;

namespace Arzenal.Store.Api.TestIntegration.Integration;

public class SharedMySqlIsolationTests
{
    [Fact]
    public async Task ConcurrentDatabaseLeases_ShareOneContainerButNotTheirRows()
    {
        var leases = await Task.WhenAll(MySqlTestDatabase.CreateAsync(), MySqlTestDatabase.CreateAsync());
        await using var first = leases[0];
        await using var second = leases[1];
        Assert.Equal(first.ContainerId, second.ContainerId);
        Assert.NotEqual(first.StoreName, second.StoreName);
        await using var firstConnection = new MySqlConnection(first.StoreConnectionString);
        await using var secondConnection = new MySqlConnection(second.StoreConnectionString);
        await firstConnection.OpenAsync();
        await secondConnection.OpenAsync();
        await using var createFirst = new MySqlCommand("CREATE TABLE Probe (Id INT PRIMARY KEY); INSERT INTO Probe VALUES (1);", firstConnection);
        await using var createSecond = new MySqlCommand("CREATE TABLE Probe (Id INT PRIMARY KEY);", secondConnection);
        await createFirst.ExecuteNonQueryAsync();
        await createSecond.ExecuteNonQueryAsync();
        await using var count = new MySqlCommand("SELECT COUNT(*) FROM Probe;", secondConnection);
        Assert.Equal(0L, Convert.ToInt64(await count.ExecuteScalarAsync()));
    }

    [Fact]
    public async Task DisposingALease_RemovesOnlyItsOwnStoreAndAuthDatabases()
    {
        await using var first = await MySqlTestDatabase.CreateAsync(includeAuth: true);
        await using var other = await MySqlTestDatabase.CreateAsync(includeAuth: true);
        await first.DisposeAsync();
        // Le second appel doit rester sans effet.
        await first.DisposeAsync();
        await using var connection = new MySqlConnection(other.StoreConnectionString);
        await connection.OpenAsync();
        await using var count = new MySqlCommand("SELECT COUNT(*) FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME IN (@store, @auth);", connection);
        count.Parameters.AddWithValue("@store", first.StoreName);
        count.Parameters.AddWithValue("@auth", first.AuthName);
        Assert.Equal(0L, Convert.ToInt64(await count.ExecuteScalarAsync()));
        count.Parameters["@store"].Value = other.StoreName;
        count.Parameters["@auth"].Value = other.AuthName;
        Assert.Equal(2L, Convert.ToInt64(await count.ExecuteScalarAsync()));
    }

    [Fact]
    public async Task DisposingAnApiFactory_LeavesOtherFactoriesAndTheirDatabasesRunning()
    {
        await using var first = new CustomWebApplicationFactory<Program>();
        await using var other = new CustomWebApplicationFactory<Program>();
        using var firstClient = first.CreateClient();
        using var otherClient = other.CreateClient();
        using var firstScope = first.Services.CreateScope();
        using var otherScope = other.Services.CreateScope();
        var firstDb = firstScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var otherDb = otherScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var firstName = firstDb.Database.GetDbConnection().Database;
        var firstAuth = firstScope.ServiceProvider.GetRequiredService<Arzenal.Store.Api.Infrastructure.Data.AuthDbContext>();
        var firstAuthName = firstAuth.Database.GetDbConnection().Database;
        Assert.NotEqual(firstName, otherDb.Database.GetDbConnection().Database);
        await first.DisposeAsync();

        Assert.Equal(HttpStatusCode.OK, (await otherClient.GetAsync("/health/ready")).StatusCode);
        Assert.Equal(2, await otherDb.Apps.CountAsync());
        // EF modifie certaines options de connexion : nettoyer seulement la chaîne d'origine
        // laisserait des sessions inactives vers les bases supprimées, jusqu'à saturer MySQL.
        await using var connection = new MySqlConnection(otherDb.Database.GetConnectionString());
        await connection.OpenAsync();
        await using var sessions = new MySqlCommand("SELECT COUNT(*) FROM INFORMATION_SCHEMA.PROCESSLIST WHERE DB IN (@store, @auth);", connection);
        sessions.Parameters.AddWithValue("@store", firstName);
        sessions.Parameters.AddWithValue("@auth", firstAuthName);
        Assert.Equal(0L, Convert.ToInt64(await sessions.ExecuteScalarAsync()));
    }
}

public class PerTestDatabaseIsolationTests : IsolatedApiTest
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task EachTest_CanInsertTheSameIdentifierIntoItsOwnFreshDatabase(int scenario)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var id = Guid.Parse("33333333-3333-3333-3333-333333333333");
        Assert.False(await db.Apps.AnyAsync(app => app.Id == id));
        db.Apps.Add(new App { Id = id, Name = $"Isolation scenario {scenario}", Version = "1.0.0" });
        await db.SaveChangesAsync();
        Assert.True(await db.Apps.AnyAsync(app => app.Id == id));
    }
}
