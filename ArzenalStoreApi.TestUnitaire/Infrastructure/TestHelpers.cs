using Arzenal.Dto.DTOs.AppDto;
using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Store.Api.Service.Mapping.AppMaping;
using Arzenal.Store.Api.Service.Services.AppService;
using ArzenalStoreInfrastructure.Data;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace Arzenal.Store.Api.TestIntegration.Infrastructure
{
    public static class TestHelpers
    {
        public static AppService CreateAppService(ApplicationDbContext dbContext)
        {
            var mapperMock = new Mock<IAppMapper>();

            // Mock du mapping App -> ReadAppDto
            mapperMock.Setup(m => m.ToReadAppDto(It.IsAny<App>()))
                .Returns((App app) => new ReadAppDto
                {
                    Id = app.Id,
                    Name = app.Name,
                    Version = app.Version,
                    IsVisible = app.IsVisible,
                    Category = "Cat",
                    AppSize = app.AppSize,
                    ReleaseDate = app.ReleaseDate
                });

            // Mock du mapping CreateAppDto -> App
            mapperMock.Setup(m => m.ToApp(It.IsAny<CreateAppDto>()))
                .Returns((CreateAppDto dto) => new App
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name ?? throw new ArgumentException("Name is required for this test.", nameof(dto)),
                    Version = dto.Version ?? throw new ArgumentException("Version is required for this test.", nameof(dto)),
                    IsVisible = dto.IsVisible,
                    CategoryId = dto.CategoryId,
                    AppSize = dto.AppSize ?? 0,
                    ReleaseDate = DateTime.UtcNow
                });
            var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var deviceName = "TestDevice";

            // Crée un ClaimsPrincipal avec le claim userId
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()) // ou ClaimTypes.NameIdentifier selon ton code
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            // Crée le HttpContext
            var httpContext = new DefaultHttpContext();
            httpContext.User = principal;
            httpContext.Request.Headers["X-Device-Name"] = deviceName;

            // Mock IHttpContextAccessor
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);
            return new AppService(dbContext, mapperMock.Object);
        }
    }

}
