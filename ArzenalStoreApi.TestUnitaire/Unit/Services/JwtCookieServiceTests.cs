using Arzenal.Store.Api.Domain.Models.Requests;
using Arzenal.Store.Api.Service.Services.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Services
{
    public class JwtCookieServiceTests
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

        public JwtCookieServiceTests()
        {
            _configMock = new Mock<IConfiguration>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            // Setup configuration pour JWT
            _configMock.Setup(c => c["Jwt:SecretKey"]).Returns("VerySecretKey1234567890151514464419184985851841844148441849148949");
            _configMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            _configMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        }

        [Fact]
        public void GetUserIdFromCookie_NoHttpContext_ReturnsEmptyGuid()
        {
            // Arrange
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext)null!);
            var service = new JwtCookieService(_configMock.Object, _httpContextAccessorMock.Object);

            // Act
            var result = service.GetUserIdFromCookie();

            // Assert
            Assert.Equal(Guid.Empty, result);
        }

        [Fact]
        public void GetUserIdFromCookie_NoCookie_ReturnsEmptyGuid()
        {
            // Arrange
            var contextMock = new DefaultHttpContext();
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(contextMock);

            var service = new JwtCookieService(_configMock.Object, _httpContextAccessorMock.Object);

            // Act
            var result = service.GetUserIdFromCookie();

            // Assert
            Assert.Equal(Guid.Empty, result);
        }

        [Fact]
        public void GetUserIdFromCookie_InvalidToken_ThrowsUnauthorized()
        {
            var cookiesMock = new Mock<IRequestCookieCollection>();
            cookiesMock.Setup(c => c["authToken"]).Returns("invalid-token");

            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.Request.Cookies).Returns(cookiesMock.Object);

            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(contextMock.Object);

            var service = new JwtCookieService(_configMock.Object, _httpContextAccessorMock.Object);

             Assert.Throws<UnauthorizedAccessException>(
                () => service.GetUserIdFromCookie()
            );
        }




        [Fact]
        public void GetUserIdFromCookie_ReturnsUserId_WhenTokenIsValid()
        {
            // Arrange 
            var userId = Guid.NewGuid();

            var key = Encoding.UTF8.GetBytes("VerySecretKey1234567890151514464419184985851841844148441849148949");
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.WriteToken(new JwtSecurityToken(
                issuer: "TestIssuer",
                claims: new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()), new Claim(JwtRegisteredClaimNames.Aud,"web") },
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            ));

            var cookiesMock = new Mock<IRequestCookieCollection>();
            cookiesMock.Setup(c => c["authToken"]).Returns(token);

            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.Request.Cookies).Returns(cookiesMock.Object);

            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(contextMock.Object);

            var service = new JwtCookieService(_configMock.Object, _httpContextAccessorMock.Object);

            // Act 
            var result = service.GetUserIdFromCookie();

            // Assert 
            Assert.Equal(userId, result);
        }

        [Fact]
        public void GetCurrentCookie_NoHttpContext_ReturnsFalse()
        {
            // Arrange
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext)null!);
            var service = new JwtCookieService(_configMock.Object, _httpContextAccessorMock.Object);

            // Act
            var result = service.GetCurrentCookie("anytoken");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetCurrentCookie_NoCookie_ReturnsFalse()
        {
            // Arrange
            var contextMock = new DefaultHttpContext();
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(contextMock);

            var service = new JwtCookieService(_configMock.Object, _httpContextAccessorMock.Object);

            // Act
            var result = service.GetCurrentCookie("anytoken");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetCurrentCookie_ReturnsFalse_WhenTokenDoesNotMatch()
        {
            // Arrange 
            var cookieData = new RefreshTokenCookieData { Token = "othertoken" };
            var encoded = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(cookieData));

            var cookiesMock = new Mock<IRequestCookieCollection>();
            cookiesMock.Setup(c => c["refreshToken"]).Returns(encoded);

            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.Request.Cookies).Returns(cookiesMock.Object);

            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(contextMock.Object);

            var service = new JwtCookieService(_configMock.Object, _httpContextAccessorMock.Object);

            // Act 
            var result = service.GetCurrentCookie("mytoken");

            // Assert 
            Assert.False(result);
        }

        [Fact]
        public void GetCurrentCookie_ReturnsTrue_WhenTokenMatches()
        {
            // Arrange 
            var cookieData = new RefreshTokenCookieData { Token = "mytoken" };
            var encoded = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(cookieData));

            var cookiesMock = new Mock<IRequestCookieCollection>();
            cookiesMock.Setup(c => c["refreshToken"]).Returns(encoded);

            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.Request.Cookies).Returns(cookiesMock.Object);

            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(contextMock.Object);

            var service = new JwtCookieService(_configMock.Object, _httpContextAccessorMock.Object);

            // Act 
            var result = service.GetCurrentCookie("mytoken");

            // Assert 
            Assert.True(result);
        }
    }
}
