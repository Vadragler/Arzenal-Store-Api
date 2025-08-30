using ArzenalStoreApi.Services.CookieService;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using Xunit;

namespace TestArzenalStoreApi.Unit.Services
{
    public class CookieServiceTests
    {
        private readonly CookieService _service;

        public CookieServiceTests()
        {
            _service = new CookieService();
        }

        [Fact]
        public void GetRefreshToken_ReturnsToken_WhenCookieExists()
        {
            var cookies = new Mock<IRequestCookieCollection>();
            cookies.Setup(c => c["refreshToken"]).Returns("refresh123");

            var request = new Mock<HttpRequest>();
            request.Setup(r => r.Cookies).Returns(cookies.Object);

            var token = _service.GetRefreshToken(request.Object);

            Assert.Equal("refresh123", token);
        }

        [Fact]
        public void GetAuthToken_ReturnsToken_WhenCookieExists()
        {
            var cookies = new Mock<IRequestCookieCollection>();
            cookies.Setup(c => c["authToken"]).Returns("jwt123");

            var request = new Mock<HttpRequest>();
            request.Setup(r => r.Cookies).Returns(cookies.Object);

            var token = _service.GetAuthToken(request.Object);

            Assert.Equal("jwt123", token);
        }

        [Fact]
        public void GetAuthToken_ThrowsUnauthorizedAccessException_WhenCookieMissing()
        {
            var cookies = new Mock<IRequestCookieCollection>();
            cookies.Setup(c => c["authToken"]).Returns((string)null);

            var request = new Mock<HttpRequest>();
            request.Setup(r => r.Cookies).Returns(cookies.Object);

            var ex = Assert.Throws<UnauthorizedAccessException>(() => _service.GetAuthToken(request.Object));
            Assert.Equal("No auth token provided in cookies.", ex.Message);
        }

        [Fact]
        public void SetAuthCookies_SetsBothCookies()
        {
            var responseCookies = new Mock<IResponseCookies>();
            var response = new Mock<HttpResponse>();
            response.Setup(r => r.Cookies).Returns(responseCookies.Object);

            _service.SetAuthCookies(response.Object, "jwt123", "refresh123");

            responseCookies.Verify(c => c.Append("refreshToken", "refresh123", It.IsAny<CookieOptions>()), Times.Once);
            responseCookies.Verify(c => c.Append("authToken", "jwt123", It.IsAny<CookieOptions>()), Times.Once);
        }

        [Fact]
        public void DeleteAuthCookies_DeletesBothCookies()
        {
            var responseCookies = new Mock<IResponseCookies>();
            var response = new Mock<HttpResponse>();
            response.Setup(r => r.Cookies).Returns(responseCookies.Object);

            _service.DeleteAuthCookies(response.Object);

            responseCookies.Verify(c => c.Delete("refreshToken"), Times.Once);
            responseCookies.Verify(c => c.Delete("authToken"), Times.Once);
        }

        [Fact]
        public void DeleteRefreshCookie_DeletesOnlyRefreshToken()
        {
            var responseCookies = new Mock<IResponseCookies>();
            var response = new Mock<HttpResponse>();
            response.Setup(r => r.Cookies).Returns(responseCookies.Object);

            _service.DeleterefreshCookie(response.Object);

            responseCookies.Verify(c => c.Delete("refreshToken"), Times.Once);
        }
    }
}
