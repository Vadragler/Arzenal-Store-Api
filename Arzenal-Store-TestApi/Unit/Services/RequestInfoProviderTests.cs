using ArzenalStoreApi.Services.RequestInfoProvider;
using ArzenalStoreSharedDto.DTOs.AuthDto;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestArzenalStoreApi.Unit.Services
{
    public class RequestInfoProviderTests
    {
        [Fact]
        public void GetRequestInfo_ShouldReturnDto_WithAllValues()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["User-Agent"] = "TestAgent";
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

            var request = new LoginRequestDto
            {
                DeviceName = "MyDevice",
                Fingerprint = "abc123"
            };

            var provider = new RequestInfoProvider();
            var result = provider.GetRequestInfo(context, request);

            Assert.Equal("MyDevice", result.DeviceName);
            Assert.Equal("abc123", result.Fingerprint);
            Assert.Equal("TestAgent", result.UserAgent);
            Assert.Equal("127.0.0.1", result.CreatedByIp);
        }

        [Fact]
        public void GetRequestInfo_ShouldUseDefaults_WhenHeadersMissing()
        {
            var context = new DefaultHttpContext();
            var request = new LoginRequestDto
            {
                DeviceName = "MyDevice",
                Fingerprint = "abc123"
            };

            var provider = new RequestInfoProvider();
            var result = provider.GetRequestInfo(context, request);

            Assert.Equal("Unknown User-Agent", result.UserAgent);
            Assert.Null(result.CreatedByIp);
        }

    }
}
