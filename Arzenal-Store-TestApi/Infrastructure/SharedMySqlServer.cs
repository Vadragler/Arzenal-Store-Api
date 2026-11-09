using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using MySqlConnector;
using Testcontainers.MySql;

namespace Arzenal.Store.Api.TestIntegration.Infrastructure;

/// <summary>Un seul serveur éphémère par exécution, prêt avant le lancement des scénarios.</summary>
internal static class SharedMySqlServer
{
    private static readonly object Gate = new();
    private static Task<MySqlContainer>? _server;

    public static Task<MySqlContainer> GetAsync()
    {
        lock (Gate) return _server ??= StartAsync();
    }

    private static async Task<MySqlContainer> StartAsync()
    {
        var server = new MySqlBuilder("mysql:8.4")
            .WithDatabase("arzenal_test_bootstrap")
            .WithUsername("root")
            .WithPassword($"Test-{Guid.NewGuid():N}")
            // Données jetables en RAM : évite les écritures disque des créations/suppressions répétées.
            .WithTmpfsMount("/var/lib/mysql")
            // Bases jetables : on teste les transactions, pas la résistance aux coupures de courant.
            .WithCommand("--innodb-flush-log-at-trx-commit=2", "--sync-binlog=0", "--skip-log-bin")
            // Le contrôle MySQL par défaut passe par le socket interne. L'initialisation
            // démarre un serveur temporaire sans TCP : attendre le même accès que les tests.
            .WithWaitStrategy(Wait.ForUnixContainer().AddCustomWaitStrategy(
                new HostConnectionReadiness(),
                options => options.WithInterval(TimeSpan.FromMilliseconds(250)).WithTimeout(TimeSpan.FromSeconds(90))))
            .WithLabel("arzenal.test-suite", "integration")
            .Build();
        try
        {
            await server.StartAsync().ConfigureAwait(false);
            return server;
        }
        catch
        {
            await server.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    public static async Task StopAsync()
    {
        Task<MySqlContainer>? pending;
        lock (Gate) { pending = _server; _server = null; }
        if (pending is null) return;
        // Une erreur de démarrage est déjà rapportée par les tests ; StartAsync a nettoyé le conteneur.
        if (pending.IsFaulted || pending.IsCanceled) return;
        var server = await pending.ConfigureAwait(false);
        var adminSettings = new MySqlConnectionStringBuilder(server.GetConnectionString()) { Database = "mysql", Pooling = true };
        try
        {
            await using var adminPool = new MySqlConnection(adminSettings.ConnectionString);
            await MySqlConnection.ClearPoolAsync(adminPool).ConfigureAwait(false);
        }
        finally
        {
            await server.DisposeAsync().ConfigureAwait(false);
        }
    }

    private sealed class HostConnectionReadiness : IWaitUntil
    {
        public async Task<bool> UntilAsync(IContainer container)
        {
            var settings = new MySqlConnectionStringBuilder(((MySqlContainer)container).GetConnectionString())
            {
                Pooling = false,
                ConnectionTimeout = 2,
                DefaultCommandTimeout = 2
            };
            await using var connection = new MySqlConnection(settings.ConnectionString);
            try
            {
                await connection.OpenAsync().ConfigureAwait(false);
                await using var query = new MySqlCommand("SELECT 1;", connection);
                return Convert.ToInt32(await query.ExecuteScalarAsync().ConfigureAwait(false)) == 1;
            }
            catch (MySqlException)
            {
                // Seule la préparation du serveur est réessayée. Les scénarios ne sont pas rejoués.
                return false;
            }
        }
    }
}
