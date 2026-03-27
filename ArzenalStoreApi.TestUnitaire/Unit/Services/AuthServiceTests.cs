using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.Domain.Models.Requests;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Services.Auth;
using Arzenal.Dto.DTOs.AuthDto;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Arzenal.Store.Api.Service.Services.Auth.Passwords;
using Arzenal.Store.Api.Infrastructure.Data;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<AuthDbContext> _dbContextMock;
        private readonly Mock<IRefreshTokenService> _tokenServiceMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<IPasswordService> _passwordServiceMock;
        private readonly Mock<ICookieService> _cookieServiceMock;
        private readonly Mock<IJwtCookieService> _jwtCookieServiceMock;

        public AuthServiceTests()
        {
            _dbContextMock = new Mock<AuthDbContext>(new DbContextOptions<AuthDbContext>());
            _tokenServiceMock = new Mock<IRefreshTokenService>();
            _jwtServiceMock = new Mock<IJwtService>();
            _passwordServiceMock = new Mock<IPasswordService>();
            _cookieServiceMock = new Mock<ICookieService>();
            _jwtCookieServiceMock = new Mock<IJwtCookieService>();   
        }

        private AuthDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AuthDbContext(options);
        }

        // ------------------ RegisterAsync ------------------
        [Fact]
        public async Task RegisterAsync_Success()
        {
            // Arrange
            using var db = GetDbContext();
            var invite = new InviteToken { Email = "test@test.com", Token = "token123", Used = false, ExpiresAt = DateTime.UtcNow.AddMinutes(10) };
            db.InviteTokens.Add(invite);
            await db.SaveChangesAsync();

            var passwordServiceMock = new Mock<IPasswordService>();
            passwordServiceMock.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed");

            var authService = new AuthService(
                db,
                Mock.Of<IRefreshTokenService>(),
                Mock.Of<IJwtService>(),
                passwordServiceMock.Object,
                Mock.Of<ICookieService>(),
                Mock.Of<IJwtCookieService>()
            );

            var request = new RegisterRequestDto
            {
                Email = "test@test.com",
                Username = "user",
                Password = "pass",
                Token = "token123"
            };

            // Act
            var result = await authService.RegisterAsync(request);

            // Assert
            Assert.True(result);
            Assert.True(invite.Used);       // token marqué comme utilisé
            Assert.Single(db.Users);         // utilisateur ajouté
        }

        [Fact]
        public async Task RegisterAsync_InvalidToken_Throws()
        {
            using var db = GetDbContext();
            var authService = new AuthService(
                db,
                Mock.Of<IRefreshTokenService>(),
                Mock.Of<IJwtService>(),
                Mock.Of<IPasswordService>(),
                Mock.Of<ICookieService>(),
                Mock.Of<IJwtCookieService>()
            );

            var request = new RegisterRequestDto
            {
                Email = "test@test.com",
                Username = "user",
                Password = "pass",
                Token = "token123"
            };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => authService.RegisterAsync(request));
        }

        [Fact]
        public async Task RegisterAsync_DuplicateUser_Throws()
        {
            PasswordService passwordhash = new PasswordService();
            using var db = GetDbContext();
            var invite = new InviteToken {Email= "test@test.com", Token = "token123", Used = false, ExpiresAt = DateTime.UtcNow.AddMinutes(10) };
            db.InviteTokens.Add(invite);
            db.Users.Add(new User { Email = "test@test.com", Username = "other", PasswordHash = passwordhash.Hash("pass") });
            await db.SaveChangesAsync();

            var passwordServiceMock = new Mock<IPasswordService>();
            passwordServiceMock.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed");

            var authService = new AuthService(
                db,
                Mock.Of<IRefreshTokenService>(),
                Mock.Of<IJwtService>(),
                passwordServiceMock.Object,
                Mock.Of<ICookieService>(),
                Mock.Of<IJwtCookieService>()
            );

            var request = new RegisterRequestDto
            {
                Email = "test@test.com",
                Username = "user",
                Password = "pass",
                Token = "token123"
            };

            await Assert.ThrowsAsync<DuplicateException>(() => authService.RegisterAsync(request));
        }

        // ------------------ AuthenticateAsync ------------------
        [Fact]
        public async Task AuthenticateAsync_Success()
        {
            using var db = GetDbContext();
            var user = new User { Username = "test", Id = Guid.NewGuid(), Email = "a@b.com", PasswordHash = "hashed" };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var passwordServiceMock = new Mock<IPasswordService>();
            passwordServiceMock.Setup(p => p.Verify("pass", "hashed")).Returns(true);

            var jwtMock = new Mock<IJwtService>();
            jwtMock.Setup(j => j.GenerateJwtTokenAsync(user.Id, It.IsAny<List<string>>())).ReturnsAsync("accessToken");

            var tokenServiceMock = new Mock<IRefreshTokenService>();
            tokenServiceMock.Setup(t => t.GenerateRefreshToken(It.IsAny<CreateRefreshTokenDto>()))
                .ReturnsAsync(new RefreshTokenCookieData { Token = "refreshToken", UserId = user.Id });

            var responseMock = new Mock<HttpResponse>();
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.Request.Headers["User-Agent"]).Returns("agent");
            responseMock.Setup(r => r.HttpContext).Returns(contextMock.Object);

            var authService = new AuthService(
                db,
                tokenServiceMock.Object,
                jwtMock.Object,
                passwordServiceMock.Object,
                Mock.Of<ICookieService>(),
                Mock.Of<IJwtCookieService>()
            );

            var dto = new CreateRefreshTokenDto { DeviceName = "PC", Fingerprint = "fp" };

            var (access, refresh) = await authService.AuthenticateAsync(responseMock.Object, "a@b.com", "pass", dto);

            Assert.Equal("accessToken", access);
            Assert.Equal("refreshToken", refresh);
        }

        [Fact]
        public async Task AuthenticateAsync_InvalidPassword_Throws()
        {
            using var db = GetDbContext();
            var user = new User { Username = "test", Id = Guid.NewGuid(), Email = "a@b.com", PasswordHash = "hashed" };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var passwordServiceMock = new Mock<IPasswordService>();
            passwordServiceMock.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            var authService = new AuthService(
                db,
                Mock.Of<IRefreshTokenService>(),
                Mock.Of<IJwtService>(),
                passwordServiceMock.Object,
                Mock.Of<ICookieService>(),
                Mock.Of<IJwtCookieService>()
            );

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                authService.AuthenticateAsync(Mock.Of<HttpResponse>(), "a@b.com", "wrongpass", new CreateRefreshTokenDto()));
        }

        // ------------------ RotateRefreshTokenAsync ------------------
        [Fact]
        public async Task RotateRefreshTokenAsync_MissingToken_Throws()
        {
            var jwtCookieMock = new Mock<IJwtCookieService>();
            var cookieMock = new Mock<ICookieService>();
            var authService = new AuthService(
                GetDbContext(),
                Mock.Of<IRefreshTokenService>(),
                Mock.Of<IJwtService>(),
                Mock.Of<IPasswordService>(),
                cookieMock.Object,
                jwtCookieMock.Object
            );

            var contextMock = new DefaultHttpContext();
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => authService.RotateRefreshTokenAsync(contextMock));
        }

        // ------------------ RevokeRefreshTokenAsync ------------------
        [Fact]
        public async Task RevokeRefreshTokenAsync_MissingToken_Throws()
        {
            var jwtCookieMock = new Mock<IJwtCookieService>();
            var cookieMock = new Mock<ICookieService>();
            cookieMock.Setup(c => c.GetRefreshToken(It.IsAny<HttpRequest>())).Returns((RefreshTokenCookieData?)null);

            var authService = new AuthService(
                GetDbContext(),
                Mock.Of<IRefreshTokenService>(),
                Mock.Of<IJwtService>(),
                Mock.Of<IPasswordService>(),
                cookieMock.Object,
                jwtCookieMock.Object
            );

            var contextMock = new DefaultHttpContext();
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => authService.RevokeRefreshTokenAsync(contextMock));
        }

        // ------------------ LogoutAsync ------------------
        [Fact]
        public async Task LogoutAsync_WithToken_DeletesToken()
        {
            using var db = GetDbContext();
            var userId = Guid.NewGuid();
            var refreshToken = new RefreshToken { Token = "tok", UserId = userId, Fingerprint = "fp", DeviceName = "PC" };
            db.RefreshTokens.Add(refreshToken);
            await db.SaveChangesAsync();

            var jwtCookieMock = new Mock<IJwtCookieService>();
            jwtCookieMock.Setup(j => j.GetUserIdFromCookie()).Returns(userId);

            var cookieMock = new Mock<ICookieService>();
            cookieMock.Setup(c => c.GetRefreshToken(It.IsAny<HttpRequest>())).Returns(new RefreshTokenCookieData { Token = "tok", UserId = userId });

            var tokenServiceMock = new Mock<IRefreshTokenService>();
            tokenServiceMock.Setup(t => t.RevokeRefreshTokenAsync("tok", userId)).ReturnsAsync(true);

            var authService = new AuthService(
                db,
                tokenServiceMock.Object,
                Mock.Of<IJwtService>(),
                Mock.Of<IPasswordService>(),
                cookieMock.Object,
                jwtCookieMock.Object
            );

            var contextMock = new DefaultHttpContext();
            await authService.LogoutAsync(contextMock);

            Assert.Empty(db.RefreshTokens.Where(rt => rt.UserId == userId && rt.Token == "tok"));
        }
    }
}
