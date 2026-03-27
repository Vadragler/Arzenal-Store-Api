using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.Service.Services.Auth.RequestInfo;
using Arzenal.Dto.DTOs.AuthDto;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Arzenal.Store.Api.Infrastructure.Data;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Services
{
    public class RequestInfoProviderTests
    {
        private AuthDbContext GetDbContext(string name)
        {
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(databaseName: name)
                .Options;
            return new AuthDbContext(options);
        }

        [Fact]
        public async Task GetRequestInfo_ShouldReturnDto_WithAllValues()
        {
            // Arrange
            var db = GetDbContext(nameof(GetRequestInfo_ShouldReturnDto_WithAllValues));
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@email.com",
                Username = "testuser",
                PasswordHash = "dummyhash"
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var context = new DefaultHttpContext();
            context.Request.Headers["User-Agent"] = "TestAgent";
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

            var request = new LoginRequestDto
            {
                Email = "test@email.com",
                DeviceName = "MyDevice",
                Fingerprint = "abc123"
            };

            var provider = new RequestInfoProvider(db);

            // Act
            var result = await provider.GetRequestInfo(context, request);

            // Assert
            Assert.Equal("MyDevice", result.DeviceName);
            Assert.Equal("abc123", result.Fingerprint);
            Assert.Equal("TestAgent", result.UserAgent);
            Assert.Equal("127.0.0.1", result.CreatedByIp);
            Assert.Equal(user.Id, result.UserId);
        }

        [Fact]
        public async Task GetRequestInfo_ShouldUseDefaults_WhenHeadersMissing()
        {
            // Arrange
            var db = GetDbContext(nameof(GetRequestInfo_ShouldUseDefaults_WhenHeadersMissing));
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@email.com",
                Username = "testuser",
                PasswordHash = "dummyhash"
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var context = new DefaultHttpContext();
            var request = new LoginRequestDto
            {
                Email = "test@email.com",
                DeviceName = "MyDevice",
                Fingerprint = "abc123"
            };

            var provider = new RequestInfoProvider(db);

            // Act
            var result = await provider.GetRequestInfo(context, request);

            // Assert
            Assert.Equal("Unknown User-Agent", result.UserAgent);
            Assert.Null(result.CreatedByIp);
        }
    }
}
