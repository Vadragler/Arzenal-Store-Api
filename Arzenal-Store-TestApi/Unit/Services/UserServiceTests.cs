using ArzenalStoreApi.Data;
using ArzenalStoreApi.Exceptions;
using ArzenalStoreApi.Models;
using ArzenalStoreApi.Services.PasswordService;
using ArzenalStoreApi.Services.UserService;
using ArzenalStoreSharedDto.DTOs.AccountDto;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestArzenalStoreApi.Unit.Services
{
    public class UserServiceTests
    {
        private AuthDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AuthDbContext(options);

            context.Users.AddRange(
                new User { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Username = "Alice", Email = "alice@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123") },
                new User { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Username = "Bob", Email = "bob@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("secret") }
            );

            context.SaveChanges();
            return context;
        }

        [Fact]
        public async Task FindByIdAsync_ReturnsUser()
        {
            var context = GetDbContext();
            var passwordService = new Mock<IPasswordService>().Object;
            var service = new UserService(context, passwordService);
            var user = await service.FindByIdAsync("11111111-1111-1111-1111-111111111111");

            Assert.NotNull(user);
            Assert.Equal("Alice", user.Username);
        }

        [Fact]
        public async Task FindByIdAsync_ThrowsNotFound_WhenUserMissing()
        {
            var context = GetDbContext();
            var passwordServiceMock = new Mock<IPasswordService>().Object;
            var service = new UserService(context, passwordServiceMock);

            await Assert.ThrowsAsync<NotFoundException>(() => service.FindByIdAsync(Guid.NewGuid().ToString()));
        }

        [Fact]
        public async Task DeleteAsync_RemovesUser()
        {
            var context = GetDbContext();
            var passwordServiceMock = new Mock<IPasswordService>().Object;
            var service = new UserService(context, passwordServiceMock);

            await service.DeleteAsync("22222222-2222-2222-2222-222222222222");

            var deleted = await context.Users.FindAsync(Guid.Parse("22222222-2222-2222-2222-222222222222"));
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_ThrowsUnauthorized_WhenUserMissing()
        {
            var context = GetDbContext();
            var passwordServiceMock = new Mock<IPasswordService>().Object;
            var service = new UserService(context, passwordServiceMock);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeleteAsync(Guid.NewGuid().ToString()));
        }

        [Fact]
        public async Task PatchAsync_UpdatesUserData()
        {
            var context = GetDbContext();
            var passwordServiceMock = new Mock<IPasswordService>();
            passwordServiceMock.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            passwordServiceMock.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashedNewPassword");

            var service = new UserService(context, passwordServiceMock.Object);

            var updateDto = new UpdateAccountDto
            {
                Username = "AliceUpdated",
                Email = "alice2@test.com",
                ActualPassword = "password123",
                NewPassword = "newpassword"
            };

            await service.PatchAsync("11111111-1111-1111-1111-111111111111", updateDto);

            var user = await context.Users.FindAsync(Guid.Parse("11111111-1111-1111-1111-111111111111"));
            Assert.Equal("AliceUpdated", user.Username);
            Assert.Equal("alice2@test.com", user.Email);
            Assert.Equal("hashedNewPassword", user.PasswordHash);
        }

        [Fact]
        public async Task PatchAsync_ThrowsUnauthorized_WhenPasswordIncorrect()
        {
            var context = GetDbContext();
            var passwordServiceMock = new Mock<IPasswordService>();
            passwordServiceMock.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            var service = new UserService(context, passwordServiceMock.Object);

            var updateDto = new UpdateAccountDto
            {
                ActualPassword = "wrongpassword",
                NewPassword = "newpassword"
            };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.PatchAsync("11111111-1111-1111-1111-111111111111", updateDto));
        }
    }
}
