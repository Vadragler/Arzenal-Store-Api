using Arzenal.Store.Api.Domain.Models.Requests;
using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Store.Api.Service.Services.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ArzenalStoreApi.TestUnitaire.Unit.Services;

public class JwtCookieServiceTests
{
    private static IConfiguration CreateConfiguration() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = "VerySecretKey1234567890151514464419184985851841844148441849148949",
            ["Jwt:Issuer"] = "TestIssuer"
        })
        .Build();

    private static IRefreshTokenHasher CreateHasher()
    {
        var hasher = new Mock<IRefreshTokenHasher>();
        hasher.Setup(service => service.Hash(It.IsAny<string>()))
            .Returns((string token) => $"hash:{token}");
        return hasher.Object;
    }

    private static JwtCookieService CreateService(HttpContext? context, ICookieService? cookies = null) => new(
        CreateConfiguration(),
        Mock.Of<IHttpContextAccessor>(accessor => accessor.HttpContext == context),
        cookies ?? Mock.Of<ICookieService>(),
        CreateHasher());

    [Fact]
    public void GetUserIdFromCookie_ReturnsEmpty_WhenThereIsNoContextOrCookie()
    {
        Assert.Equal(Guid.Empty, CreateService(null).GetUserIdFromCookie());
        Assert.Equal(Guid.Empty, CreateService(new DefaultHttpContext()).GetUserIdFromCookie());
    }

    [Fact]
    public void GetUserIdFromCookie_ReturnsTheValidatedJwtSubject()
    {
        var userId = Guid.NewGuid();
        var key = Encoding.UTF8.GetBytes("VerySecretKey1234567890151514464419184985851841844148441849148949");
        var jwt = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: "TestIssuer",
            claims: [new Claim(ClaimTypes.NameIdentifier, userId.ToString()), new Claim(JwtRegisteredClaimNames.Aud, "web")],
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)));
        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = $"authToken={jwt}";

        Assert.Equal(userId, CreateService(context).GetUserIdFromCookie());
    }

    [Fact]
    public void GetUserIdFromCookie_RejectsAnInvalidJwt()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = "authToken=invalid";

        Assert.Throws<UnauthorizedAccessException>(() => CreateService(context).GetUserIdFromCookie());
    }

    [Fact]
    public void GetCurrentCookie_UsesTheHashInsteadOfComparingRawTokens()
    {
        var context = new DefaultHttpContext();
        var cookies = new Mock<ICookieService>();
        cookies.Setup(service => service.GetRefreshToken(It.IsAny<HttpRequest>()))
            .Returns(new RefreshTokenCookieData { Token = "raw-token" });

        var service = CreateService(context, cookies.Object);

        Assert.True(service.GetCurrentCookie("hash:raw-token"));
        Assert.False(service.GetCurrentCookie("hash:other-token"));
    }

    [Fact]
    public void GetCurrentCookie_ReturnsFalseWithoutARefreshCookie()
    {
        var context = new DefaultHttpContext();
        var cookies = new Mock<ICookieService>();
        cookies.Setup(service => service.GetRefreshToken(It.IsAny<HttpRequest>()))
            .Returns((RefreshTokenCookieData?)null);

        Assert.False(CreateService(context, cookies.Object).GetCurrentCookie("hash:any"));
    }
}
