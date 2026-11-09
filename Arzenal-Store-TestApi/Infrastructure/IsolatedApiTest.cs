namespace Arzenal.Store.Api.TestIntegration.Infrastructure;

public abstract class IsolatedApiTest : IsolatedApiTest<CustomWebApplicationFactory<Program>>;

/// <summary>xUnit construit une instance par test : état HTTP, bases et fichiers ne sont pas partagés.</summary>
public abstract class IsolatedApiTest<TFactory> : IAsyncLifetime
    where TFactory : CustomWebApplicationFactory<Program>, new()
{
    protected TFactory Factory { get; } = new();
    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Factory.DisposeAsync().AsTask();
}
