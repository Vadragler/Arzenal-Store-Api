using Arzenal.Store.Api.Domain.Models.Requests;
using Arzenal.Store.Api.Service.Services.Auth.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Text;
using System.Text.Json;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Services
{
    public class CookieServiceTests
    {
        private readonly CookieService _service;

        public CookieServiceTests()
        {
            _service = new CookieService(CreateConfiguration());
        }

        private static IConfiguration CreateConfiguration()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection()
                .Build();
        }

        [Fact]
        public void GetRefreshToken_ReturnsToken_WhenCookieExists()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var cookieJson = "{\"Token\":\"refresh123\", \"UserId\":\"" + userId + "\"}";
            var base64Cookie = Convert.ToBase64String(Encoding.UTF8.GetBytes(cookieJson));

            var cookies = new Mock<IRequestCookieCollection>();
            cookies.Setup(c => c["refreshToken"]).Returns(base64Cookie);

            var request = new Mock<HttpRequest>();
            request.Setup(r => r.Cookies).Returns(cookies.Object);

            // Act
            var token = _service.GetRefreshToken(request.Object);

            // Assert
            Assert.Equal("refresh123", token!.Token);
            Assert.Equal(userId, token.UserId);
        }

        [Fact]
        public void GetAuthToken_ReturnsToken_WhenCookieExists()
        {
            // Arrange
            var cookies = new Mock<IRequestCookieCollection>();
            cookies.Setup(c => c["authToken"]).Returns("jwt123");

            var request = new Mock<HttpRequest>();
            request.Setup(r => r.Cookies).Returns(cookies.Object);

            // Act
            var token = _service.GetAuthToken(request.Object);

            // Assert
            Assert.Equal("jwt123", token);
        }

        [Fact]
        public void GetAuthToken_ThrowsUnauthorizedAccessException_WhenCookieMissing()
        {
            // Arrange
            var cookies = new Mock<IRequestCookieCollection>();
            cookies.Setup(c => c["authToken"]).Returns((string)null!);

            var request = new Mock<HttpRequest>();
            request.Setup(r => r.Cookies).Returns(cookies.Object);

            // Act & Assert
            var ex = Assert.Throws<UnauthorizedAccessException>(() => _service.GetAuthToken(request.Object));
            Assert.Equal("No auth token provided in cookies.", ex.Message);
        }

        [Fact]
        public void SetAuthCookies_SetsBothCookies()
        {
            // Arrange
            var responseCookies = new Mock<IResponseCookies>();
            var response = new Mock<HttpResponse>();
            response.Setup(r => r.Cookies).Returns(responseCookies.Object);
            Guid userId = Guid.NewGuid();
            var refreshTokenData = new RefreshTokenCookieData
            {
                Token = "refresh123",
                UserId = userId
            };

            // Act
            _service.SetAuthCookies(response.Object, "jwt123", refreshTokenData);

            // Assert
            responseCookies.Verify(c => c.Append(
                "authToken",
                "jwt123",
                It.IsAny<CookieOptions>()),
                Times.Once);


            var expectedRefreshToken = Convert.ToBase64String(
                JsonSerializer.SerializeToUtf8Bytes(new RefreshTokenCookieData
                {
                    Token = "refresh123",
                    UserId = userId
                })
            );
            // Vérifie refreshToken en testant la valeur encodée
            responseCookies.Verify(c => c.Append(
            "refreshToken",
            expectedRefreshToken,
            It.IsAny<CookieOptions>()),
            Times.Once);
        }

        [Fact]
        public void DeleteAuthCookies_DeletesBothCookies()
        {
            // Arrange
            var requestCookies = new Mock<IRequestCookieCollection>();
            requestCookies.Setup(c => c.ContainsKey("authToken")).Returns(true);
            requestCookies.Setup(c => c.ContainsKey("refreshToken")).Returns(true);

            var request = new Mock<HttpRequest>();
            request.Setup(r => r.Cookies).Returns(requestCookies.Object);

            var responseCookies = new Mock<IResponseCookies>();

            var response = new Mock<HttpResponse>();
            response.Setup(r => r.Cookies).Returns(responseCookies.Object);

            var context = new Mock<HttpContext>();
            context.Setup(c => c.Request).Returns(request.Object);
            context.Setup(c => c.Response).Returns(response.Object);

            // Act
            _service.DeleteAuthCookies(context.Object);

            // Assert
            responseCookies.Verify(c => c.Delete("authToken", It.IsAny<CookieOptions>()), Times.Once);
            responseCookies.Verify(c => c.Delete("refreshToken", It.IsAny<CookieOptions>()), Times.Once);
        }

        [Fact]
        public void DeleteRefreshCookie_DeletesOnlyRefreshToken()
        {
            // Arrange
            var responseCookies = new Mock<IResponseCookies>();
            var response = new Mock<HttpResponse>();
            response.Setup(r => r.Cookies).Returns(responseCookies.Object);

            // Act
            _service.DeleteRefreshCookie(response.Object);

            // Assert
            responseCookies.Verify(c => c.Delete("refreshToken"), Times.Once);
        }
    }
}
