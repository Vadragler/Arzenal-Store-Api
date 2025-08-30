namespace TestArzenalStoreApi.Infrastructure
{
    [CollectionDefinition("SharedDatabaseCollection")]
    public class SharedDatabaseCollection : ICollectionFixture<CustomWebApplicationFactory<Program>>
    {
        // Ne rien mettre ici
    }
}
