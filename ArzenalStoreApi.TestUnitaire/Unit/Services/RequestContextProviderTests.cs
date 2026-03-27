using Arzenal.Store.Api.Service.Services.Auth.RequestContext;
using Microsoft.AspNetCore.Http;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Services
{
    public class RequestContextProviderTests
    {
        [Fact]
        public void Get_ShouldReturnClientContext_WithAllValues()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["User-Agent"] = "TestAgent";
            context.Request.Headers["X-Fingerprint"] = "12345";
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            var provider = new RequestContextProvider();

            // Act
            var result = provider.Get(context);

            // Assert
            Assert.Equal("TestAgent", result.UserAgent);
            Assert.Equal("127.0.0.1", result.IpAddress);
            Assert.Equal("12345", result.Fingerprint);
        }

        [Fact]
        public void Get_ShouldReturnDefaults_WhenHeadersMissing()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var provider = new RequestContextProvider();

            // Act
            var result = provider.Get(context);

            // Assert
            Assert.Equal("Unknown User-Agent", result.UserAgent);
            Assert.Equal("unknown", result.IpAddress);
            Assert.Null(result.Fingerprint);
        }
    }
}
