using Arzenal.Dto.DTOs.AuthDto;
using Arzenal.Store.Api.Domain.Models.Requests;
using Arzenal.Store.Api.Infrastructure.Data;
using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Store.Api.Service.Models.Auth;
using Arzenal.Store.Api.Service.Services.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Net;
using System.Security.Claims;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Services;

public class RefreshTokenSecurityTests
{
    [Fact]
    public async Task GenerateRefreshTokenAsync_UsesUserIdFromAuthenticatedPrincipal()
    {
        var authenticatedUserId = Guid.NewGuid();
        RefreshTokenCreationRequest? capturedRequest = null;
        var refreshTokenService = new Mock<IRefreshTokenService>();
        refreshTokenService
            .Setup(service => service.GenerateRefreshToken(It.IsAny<RefreshTokenCreationRequest>()))
            .Callback<RefreshTokenCreationRequest>(request => capturedRequest = request)
            .ReturnsAsync(new RefreshTokenCookieData
            {
                Token = "refresh-token",
                UserId = authenticatedUserId
            });

        var jwtCookieService = new Mock<IJwtCookieService>();
        var jwtService = new Mock<IJwtService>();
        jwtService
            .Setup(service => service.GenerateJwtTokenAsync(authenticatedUserId, It.IsAny<List<string>>()))
            .ReturnsAsync("access-token");

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var dbContext = new AuthDbContext(options);
        var authService = new AuthService(
            dbContext,
            refreshTokenService.Object,
            jwtService.Object,
            Mock.Of<IPasswordService>(),
            Mock.Of<ICookieService>(),
            jwtCookieService.Object);

        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, authenticatedUserId.ToString()) },
            "HeaderJwt"));
        context.Request.Headers["User-Agent"] = "ArzenalStoreManager";
        context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");

        await authService.GenerateRefreshTokenAsync(context, new CreateRefreshTokenDto
        {
            DeviceName = "security-test-device",
            Fingerprint = "security-test-fingerprint"
        });

        Assert.NotNull(capturedRequest);
        Assert.Equal(authenticatedUserId, capturedRequest.UserId);
        Assert.Equal("ArzenalStoreManager", capturedRequest.UserAgent);
        Assert.Equal("127.0.0.1", capturedRequest.CreatedByIp);
        jwtCookieService.Verify(service => service.GetUserIdFromCookie(), Times.Never);
    }
}
