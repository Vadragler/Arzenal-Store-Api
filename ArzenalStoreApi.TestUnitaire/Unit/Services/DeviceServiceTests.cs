using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Services.Accounts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Arzenal.Store.Api.Infrastructure.Data;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Services
{
    public class DeviceServiceTests
    {
        private AuthDbContext CreateInMemoryContext(string testName)
        {
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(testName)
                .EnableSensitiveDataLogging()
                .Options;

            return new AuthDbContext(options);
        } 

        [Fact]
        public async Task GetAllDevice_ReturnsListOfDevices()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var db = new AuthDbContext(options);

            var userId = Guid.NewGuid();
            db.RefreshTokens.AddRange(
                new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    DeviceName = "PC",
                    CreatedByIp = "127.0.0.1",
                    LastUsed = DateTime.UtcNow,
                    Type = "PC",
                    Token = "token1",
                    Fingerprint = "fingerprint1"
                },
                new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    DeviceName = "Mobile",
                    CreatedByIp = "127.0.0.2",
                    LastUsed = DateTime.UtcNow,
                    Type = "Mobile",
                    Token = "token2",
                    Fingerprint = "fingerprint2"
                }
            );
            await db.SaveChangesAsync();

            var jwtMock = new Mock<IJwtCookieService>();
            jwtMock.Setup(s => s.GetUserIdFromCookie()).Returns(userId);
            jwtMock.Setup(s => s.GetCurrentCookie(It.IsAny<string>())).Returns(true);

            var service = new DeviceService(db, jwtMock.Object);

            // Act
            var result = await service.GetAllDevice(new DefaultHttpContext().Request);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, d => Assert.True(d.IsCurrentDevice));
        }

        [Fact]
        public async Task RevokeDevice_Success()
        {
            // Arrange
            var db = CreateInMemoryContext(nameof(RevokeDevice_Success));
            var userId = Guid.NewGuid();
            var deviceId = Guid.NewGuid();
            var token = Guid.NewGuid();

            db.RefreshTokens.Add(new RefreshToken
            {
                Id = deviceId,
                Token = token.ToString(),
                UserId = userId,
                Fingerprint = "fingerprint",
                DeviceName = "PC"
            });
            await db.SaveChangesAsync();

            var jwtMock = new Mock<IJwtCookieService>();
            jwtMock.Setup(s => s.GetUserIdFromCookie()).Returns(userId);

            var service = new DeviceService(db, jwtMock.Object);

            // Act
            var result = await service.RevokeDevice(deviceId);

            // Assert
            Assert.True(result);
            Assert.Empty(db.RefreshTokens);
        }


        [Fact]
        public async Task RevokeDevice_ThrowsNotFoundException_WhenTokenNotFound()
        {
            // Arrange
            var db = CreateInMemoryContext(nameof(RevokeDevice_ThrowsNotFoundException_WhenTokenNotFound));
            var jwtMock = new Mock<IJwtCookieService>();
            jwtMock.Setup(s => s.GetUserIdFromCookie()).Returns(Guid.NewGuid());

            var service = new DeviceService(db, jwtMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => service.RevokeDevice(Guid.NewGuid()));
        }


        [Fact]
        public async Task RevokeDevice_ThrowsUnauthorizedAccessException_WhenUserNotAuthenticated()
        {
            // Arrange
            var db = CreateInMemoryContext(nameof(RevokeDevice_ThrowsUnauthorizedAccessException_WhenUserNotAuthenticated));
            var jwtMock = new Mock<IJwtCookieService>();
            jwtMock.Setup(s => s.GetUserIdFromCookie()).Returns(Guid.Empty);

            var service = new DeviceService(db, jwtMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.RevokeDevice(Guid.NewGuid()));
        }
    }
}
