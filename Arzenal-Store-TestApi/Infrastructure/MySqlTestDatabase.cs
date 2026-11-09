using System.Text.RegularExpressions;
using MySqlConnector;

namespace Arzenal.Store.Api.TestIntegration.Infrastructure;

/// <summary>Possède uniquement les bases aux noms aléatoires créées pour ce scénario.</summary>
internal sealed class MySqlTestDatabase : IAsyncDisposable, IDisposable
{
    private readonly string _adminConnectionString;
    private int _disposed;

    private MySqlTestDatabase(string serverConnection, string containerId, bool includeAuth)
    {
        var suffix = Guid.NewGuid().ToString("N")[..16];
        StoreName = $"arzenal_test_{suffix}_store";
        AuthName = includeAuth ? $"arzenal_test_{suffix}_auth" : null;
        ContainerId = containerId;
        // Un petit pool d'administration commun évite de renégocier une connexion à chaque CREATE/DROP.
        var builder = new MySqlConnectionStringBuilder(serverConnection) { Database = "mysql", Pooling = true };
        _adminConnectionString = builder.ConnectionString;
        // Expliciter les options ajoutées par Pomelo évite un second pool caché derrière
        // une chaîne légèrement différente. DisposeAsync peut alors fermer le pool réellement utilisé.
        builder.AllowUserVariables = true;
        builder.UseAffectedRows = false;
        builder.TreatTinyAsBoolean = true;
        builder.Database = StoreName;
        StoreConnectionString = builder.ConnectionString;
        if (AuthName is not null)
        {
            builder.Database = AuthName;
            AuthConnectionString = builder.ConnectionString;
        }
    }

    public string StoreName { get; }
    public string? AuthName { get; }
    public string ContainerId { get; }
    public string StoreConnectionString { get; }
    public string? AuthConnectionString { get; }

    public static async Task<MySqlTestDatabase> CreateAsync(bool includeAuth = false)
    {
        var server = await SharedMySqlServer.GetAsync().ConfigureAwait(false);
        var databases = new MySqlTestDatabase(server.GetConnectionString(), server.Id, includeAuth);
        try
        {
            await databases.ExecuteForOwnedDatabasesAsync("CREATE DATABASE").ConfigureAwait(false);
            return databases;
        }
        catch
        {
            await databases.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    private async Task ExecuteForOwnedDatabasesAsync(string operation)
    {
        await using var admin = new MySqlConnection(_adminConnectionString);
        await admin.OpenAsync().ConfigureAwait(false);
        foreach (var name in new[] { StoreName, AuthName })
        {
            if (name is null) continue;
            if (!Regex.IsMatch(name, "^arzenal_test_[a-f0-9]{16}_(store|auth)$", RegexOptions.CultureInvariant))
                throw new InvalidOperationException("Nom de base de test non autorisé.");
            await using var command = new MySqlCommand($"{operation} `{name}`;", admin);
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        foreach (var connectionString in new[] { StoreConnectionString, AuthConnectionString })
        {
            if (connectionString is null) continue;
            await using var connection = new MySqlConnection(connectionString);
            await MySqlConnection.ClearPoolAsync(connection).ConfigureAwait(false);
        }
        await ExecuteForOwnedDatabasesAsync("DROP DATABASE IF EXISTS").ConfigureAwait(false);
    }

    public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();
}
