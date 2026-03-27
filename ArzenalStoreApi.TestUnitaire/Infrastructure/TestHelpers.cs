using Arzenal.Store.Api.Domain.Models;
using ArzenalStoreInfrastructure.Data;
using Arzenal.Store.Api.Service.Mapping.AppMaping;
using Arzenal.Store.Api.Service.Services.AppService;
using Arzenal.Dto.DTOs.AppDto;
using Moq;

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
                    Name = dto.Name,
                    Version = dto.Version,
                    IsVisible = dto.IsVisible,
                    CategoryId = dto.CategoryId,
                    AppSize = (long)dto.AppSize,
                    ReleaseDate = DateTime.UtcNow
                });

            return new AppService(dbContext, mapperMock.Object);
        }
    }

}
