namespace TestArzenalStoreApi.Infrastructure
{
    public static class TestHelpers
    {
        public static (AlternateCustomWebApplicationFactory<TStartup> factory, HttpClient client)
            CreateClientWithEmptyDb<TStartup>() where TStartup : class
        {
            var factory = new AlternateCustomWebApplicationFactory<TStartup>();
            var client = factory.CreateClient();
            return (factory, client);
        }
    }

}
