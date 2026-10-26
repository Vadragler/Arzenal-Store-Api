using Arzenal.Store.Api.Service.Services.Auth.Tokens;
using Microsoft.Extensions.Configuration;

namespace ArzenalStoreApi.TestUnitaire.Unit.Services;

public class HmacRefreshTokenHasherTests
{
    [Fact]
    public void Hash_IsDeterministicAndDoesNotExposeTheRawToken()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RefreshToken:HashKey"] = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA="
            })
            .Build();
        var hasher = new HmacRefreshTokenHasher(configuration);

        var first = hasher.Hash("raw-refresh-token");
        var second = hasher.Hash("raw-refresh-token");

        Assert.Equal(first, second);
        Assert.NotEqual("raw-refresh-token", first);
        Assert.Equal(64, first.Length);
        Assert.NotEqual(first, hasher.Hash("other-refresh-token"));
    }
}
