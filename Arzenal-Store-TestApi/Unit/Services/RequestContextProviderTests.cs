using ArzenalStoreApi.Services.RequestContextProvider;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestArzenalStoreApi.Unit.Services
{
    public class RequestContextProviderTests
    {
        [Fact]
        public void Get_ShouldReturnClientContext_WithAllValues()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["User-Agent"] = "TestAgent";
            context.Request.Headers["X-Fingerprint"] = "12345";
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

            var provider = new RequestContextProvider();
            var result = provider.Get(context);

            Assert.Equal("TestAgent", result.UserAgent);
            Assert.Equal("127.0.0.1", result.IpAddress);
            Assert.Equal("12345", result.Fingerprint);
        }

        [Fact]
        public void Get_ShouldReturnDefaults_WhenHeadersMissing()
        {
            var context = new DefaultHttpContext();
            var provider = new RequestContextProvider();
            var result = provider.Get(context);

            Assert.Equal("Unknown User-Agent", result.UserAgent);
            Assert.Equal("unknown", result.IpAddress);
            Assert.Null(result.Fingerprint);
        }

    }
}
