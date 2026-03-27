using Arzenal.Store.Api.Domain.Models;
using Arzenal.Dto.DTOs.AppDto;
using Arzenal.Dto.DTOs.CategorieDto;
using Arzenal.Dto.DTOs.LanguageDto;
using Arzenal.Dto.DTOs.OperatingSystemDto;
using Arzenal.Dto.DTOs.TagDto;
using Arzenal.Store.Api.Service.Mapping.AppMaping;
using Arzenal.Store.Api.Service.Mapping.CategoryMapping;
using Arzenal.Store.Api.Service.Mapping.LanguageMapping;
using Arzenal.Store.Api.Service.Mapping.OperatingSystemMapping;
using Arzenal.Store.Api.Service.Mapping.TagMapping;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OperatingSystem = Arzenal.Store.Api.Domain.Models.OperatingSystem;

namespace Arzenal.Store.Api.TestIntegration.Infrastructure
{
    public static class MapperMocksFactory
    {
        public static void SetupDefaultMapperMocks(IServiceCollection services)
        {
            var mapperDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IAppMapper));
            if (mapperDescriptor != null) services.Remove(mapperDescriptor);


            var mapperMock = new Mock<IAppMapper>();
            var categoryMapperMock = new Mock<ICategoryMapper>();
            var languageMapperMock = new Mock<ILanguageMapper>();
            var operatingSystemMapperMock = new Mock<IOperatingSystemMapper>();
            var tagMapperMock = new Mock<ITagMapper>();

            // === APP MAPPER ===
            mapperMock.Setup(m => m.ToReadAppDto(It.IsAny<App>()))
                .Returns((App app) => new ReadAppDto
                {
                    Id = app.Id,
                    Name = app.Name,
                    Version = app.Version,
                    Description = app.Description,
                    IsVisible = app.IsVisible,
                    IconePath = app.Icone,
                    Category = app.Category?.Name ?? "Unknown",
                    Languages = app.AppLanguages?.Select(al => al.Language?.Name ?? "Unknown").ToList(),
                    Tags = app.AppTags?.Select(at => at.Tag?.Name ?? "Unknown").ToList(),
                    OperatingSystems = app.AppOperatingSystems?.Select(aos => aos.OperatingSystem?.Name ?? "Unknown").ToList(),
                    AppSize = app.AppSize,
                    Requirements = app.Requirements,
                    ReleaseDate = app.ReleaseDate,
                    LastUpdated = app.LastUpdated
                });

            mapperMock.Setup(m => m.ToApp(It.IsAny<CreateAppDto>()))
                .Returns((CreateAppDto dto) => new App
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Version = dto.Version,
                    Description = dto.Description,
                    IsVisible = dto.IsVisible,
                    Icone = dto.IconePath,
                    CategoryId = dto.CategoryId,
                    AppSize = dto.AppSize ?? 0L,
                    Requirements = dto.Requirements,
                    LastUpdated = dto.LastUpdated,
                    AppLanguages = dto.LanguageIds?.Select(id => new AppLanguage { LanguageId = id }).ToList(),
                    AppTags = dto.TagIds?.Select(id => new AppTag { TagId = id }).ToList(),
                    AppOperatingSystems = dto.OsIds?.Select(id => new AppOperatingSystem { OSId = id }).ToList()
                });

            mapperMock.Setup(m => m.UpdateAppFromDto(It.IsAny<UpdateAppDto>(), It.IsAny<App>()))
                .Callback((UpdateAppDto dto, App app) =>
                {
                    if (dto.Name != null) app.Name = dto.Name;
                    if (dto.Version != null) app.Version = dto.Version;
                    if (dto.Description != null) app.Description = dto.Description;
                    if (dto.IsVisible.HasValue) app.IsVisible = dto.IsVisible.Value;
                    if (dto.IconePath != null) app.Icone = dto.IconePath;
                    if (dto.AppSize.HasValue) app.AppSize = dto.AppSize.Value;
                    if (dto.Requirements != null) app.Requirements = dto.Requirements;
                    if (dto.CategoryId.HasValue) app.CategoryId = dto.CategoryId.Value;
                    app.LastUpdated = DateTime.UtcNow;
                });


            // === CATEGORY MAPPER ===
            categoryMapperMock.Setup(m => m.ToReadCategoryDto(It.IsAny<Categorie>()))
                .Returns((Categorie c) => new ReadCategorieDto
                {
                    Id = c.Id,
                    Name = c.Name
                });

            categoryMapperMock.Setup(m => m.ToCategory(It.IsAny<CreateCategorieDto>()))
                .Returns((CreateCategorieDto dto) => new Categorie
                {
                    Name = dto.Name!
                });

            categoryMapperMock.Setup(m => m.UpdateCategoryFromDto(It.IsAny<UpdateCategorieDto>(), It.IsAny<Categorie>()))
                .Callback((UpdateCategorieDto dto, Categorie c) =>
                {
                    if (dto.Name != null) c.Name = dto.Name;
                });


            // === LANGUAGE MAPPER ===
            languageMapperMock.Setup(m => m.ToReadLanguageDto(It.IsAny<Language>()))
                .Returns((Language l) => new ReadLanguageDto
                {
                    Id = l.Id,
                    Name = l.Name
                });

            languageMapperMock.Setup(m => m.ToLanguage(It.IsAny<CreateLanguageDto>()))
                .Returns((CreateLanguageDto dto) => new Language
                {
                    Name = dto.Name
                });

            languageMapperMock.Setup(m => m.UpdateLanguageFromDto(It.IsAny<UpdateLanguageDto>(), It.IsAny<Language>()))
                .Callback((UpdateLanguageDto dto, Language l) =>
                {
                    if (dto.Name != null) l.Name = dto.Name;
                });


            // === OPERATING SYSTEM MAPPER ===
            operatingSystemMapperMock.Setup(m => m.ToReadOperatingSystemDto(It.IsAny<OperatingSystem>()))
                .Returns((OperatingSystem os) => new ReadOperatingSystemDto
                {
                    Id = os.Id,
                    Name = os.Name
                });

            operatingSystemMapperMock.Setup(m => m.ToOperatingSystem(It.IsAny<CreateOperatingSystemDto>()))
                .Returns((CreateOperatingSystemDto dto) => new OperatingSystem
                {
                    Name = dto.Name
                });

            operatingSystemMapperMock.Setup(m => m.UpdateOperatingSystemFromDto(It.IsAny<UpdateOperatingSystemDto>(), It.IsAny<OperatingSystem>()))
                .Callback((UpdateOperatingSystemDto dto, OperatingSystem os) =>
                {
                    if (dto.Name != null) os.Name = dto.Name;
                });


            // === TAG MAPPER ===
            tagMapperMock.Setup(m => m.ToReadTagDto(It.IsAny<Tag>()))
                .Returns((Tag t) => new ReadTagDto
                {
                    Id = t.Id,
                    Name = t.Name
                });

            tagMapperMock.Setup(m => m.ToTag(It.IsAny<CreateTagDto>()))
                .Returns((CreateTagDto dto) => new Tag
                {
                    Name = dto.Name
                });

            tagMapperMock.Setup(m => m.UpdateTagFromDto(It.IsAny<UpdateTagDto>(), It.IsAny<Tag>()))
                .Callback((UpdateTagDto dto, Tag t) =>
                {
                    if (dto.Name != null) t.Name = dto.Name;
                });




            services.AddSingleton(mapperMock.Object);
            services.AddSingleton(categoryMapperMock.Object);
            services.AddSingleton(languageMapperMock.Object);
            services.AddSingleton(operatingSystemMapperMock.Object);
            services.AddSingleton(tagMapperMock.Object);
        }
    }
}
