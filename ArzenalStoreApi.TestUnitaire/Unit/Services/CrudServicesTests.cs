using Arzenal.Dto.DTOs.CategorieDto;
using Arzenal.Dto.DTOs.LanguageDto;
using Arzenal.Dto.DTOs.OperatingSystemDto;
using Arzenal.Dto.DTOs.TagDto;
using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Store.Api.Service.Mapping.CategoryMapping;
using Arzenal.Store.Api.Service.Mapping.LanguageMapping;
using Arzenal.Store.Api.Service.Mapping.OperatingSystemMapping;
using Arzenal.Store.Api.Service.Mapping.TagMapping;
using Arzenal.Store.Api.Service.Services.CategoryService;
using Arzenal.Store.Api.Service.Services.LanguageService;
using Arzenal.Store.Api.Service.Services.OperatingSystemService;
using Arzenal.Store.Api.Service.Services.TagService;
using ArzenalStoreInfrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using OperatingSystem = Arzenal.Store.Api.Domain.Models.OperatingSystem;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Services
{
    public class CrudServicesTests
    {
        private readonly ICategoryMapper _categorymapper;
        private readonly ILanguageMapper _languagemapper;
        private readonly IOperatingSystemMapper _operatingSystemmapper;
        private readonly ITagMapper _tagmapper;

        public CrudServicesTests()
        {

            var categoryMapper = new CategoryMapper();
            _categorymapper = new CategoryMapperWrapper(categoryMapper);

            var languageMapper = new LanguageMapper();
            _languagemapper = new LanguageMapperWrapper(languageMapper);

            var osMapper = new OperatingSystemMapper();
            _operatingSystemmapper = new OperatingSystemMapperWrapper(osMapper);

            var tagMapper = new TagMapper();
            _tagmapper = new TagMapperWrapper(tagMapper);
        }

        private ApplicationDbContext GetDbContext<T>(List<T> data) where T : class
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            context.Set<T>().AddRange(data);
            context.SaveChanges();
            return context;
        }

        // ---------------- CATEGORY ----------------
        [Fact]
        public async Task CategoryService_Crud_WithExceptions_Test()
        {
            // Arrange
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
            var categories = new List<Categorie>
            {
                new Categorie { Id = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"), Name = "Cat1" },
                new Categorie { Id = Guid.Parse("a3f5d2e7-8c4b-4a1f-9e2b-7d6f5c3b2a1e"), Name = "Cat2" }
            };
            var context = GetDbContext(categories);
            var service = new CategoryService(context, _categorymapper);

            // Act & Assert
            // --- Happy Path ---
            var all = await service.GetAllAsync();
            Assert.Equal(2, all.Count());

            var cat = await service.GetByIdAsync(Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"));
            Assert.NotNull(cat);
            Assert.Equal("Cat1", cat.Name);

            var newCat = await service.CreateAsync(new CreateCategorieDto { Name = "Cat3" });
            Assert.NotNull(newCat);
            Assert.Equal("Cat3", newCat.Name);

            var updated = await service.UpdateAsync(Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"), new UpdateCategorieDto { Name = "UpdatedCat" });
            Assert.True(updated);

            var deleted = await service.DeleteAsync(Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"));
            Assert.True(deleted);




            // --- Duplicate Exceptions ---
            await Assert.ThrowsAsync<DuplicateException>(() => service.CreateAsync(new CreateCategorieDto { Name = "Cat2" }));
            await Assert.ThrowsAsync<DuplicateException>(() => service.UpdateAsync(Guid.Parse("a3f5d2e7-8c4b-4a1f-9e2b-7d6f5c3b2a1e"), new UpdateCategorieDto { Name = "Cat2" }));

            // --- NotFound Exceptions ---
            await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(Guid.Parse("d7e2f4b9-5a3c-4d6e-8f1b-9c2a7d5e3f6b")));
            await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateAsync(Guid.Parse("d7e2f4b9-5a3c-4d6e-8f1b-9c2a7d5e3f6b"), new UpdateCategorieDto { Name = "X" }));
            await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(Guid.Parse("d7e2f4b9-5a3c-4d6e-8f1b-9c2a7d5e3f6b")));
        }

        // ---------------- LANGUAGE ----------------
        [Fact]
        public async Task LanguageService_Crud_WithExceptions_Test()
        {
            // Arrange
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
            var languages = new List<Language>
            {
                new Language { Id = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"), Name = "Lang1" },
                new Language { Id = Guid.Parse("a3f5d2e7-8c4b-4a1f-9e2b-7d6f5c3b2a1e"), Name = "Lang2" }
            };
            var context = GetDbContext(languages);
            var service = new LanguageService(context, _languagemapper);

            // Act & Assert
            var all = await service.GetAllAsync();
            Assert.Equal(2, all.Count());

            var lang = await service.GetByIdAsync(Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"));
            Assert.Equal("Lang1", lang.Name);

            var newLang = await service.CreateAsync(new CreateLanguageDto { Name = "Lang3" });
            Assert.Equal("Lang3", newLang.Name);


            var updated = await service.UpdateAsync(Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"), new UpdateLanguageDto { Name = "UpdatedLang" });
            Assert.True(updated);

            var deleted = await service.DeleteAsync(Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"));
            Assert.True(deleted);




            // --- Duplicate ---
            await Assert.ThrowsAsync<DuplicateException>(() => service.CreateAsync(new CreateLanguageDto { Name = "Lang2" }));
            await Assert.ThrowsAsync<DuplicateException>(() => service.UpdateAsync(Guid.Parse("a3f5d2e7-8c4b-4a1f-9e2b-7d6f5c3b2a1e"), new UpdateLanguageDto { Name = "Lang3" }));

            // --- NotFound ---
            await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(Guid.Parse("d7e2f4b9-5a3c-4d6e-8f1b-9c2a7d5e3f6b")));
            await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateAsync(Guid.Parse("d7e2f4b9-5a3c-4d6e-8f1b-9c2a7d5e3f6b"), new UpdateLanguageDto { Name = "X" }));
            await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(Guid.Parse("d7e2f4b9-5a3c-4d6e-8f1b-9c2a7d5e3f6b")));
        }

        // ---------------- OPERATING SYSTEM ----------------
        [Fact]
        public async Task OperatingSystemService_Crud_WithExceptions_Test()
        {
            // Arrange
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
            var oss = new List<OperatingSystem>
            {
                new OperatingSystem { Id = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"), Name = "OS1" },
                new OperatingSystem { Id = Guid.Parse("a3f5d2e7-8c4b-4a1f-9e2b-7d6f5c3b2a1e"), Name = "OS2" }
            };
            var context = GetDbContext(oss);
            var service = new OperatingSystemService(context, _operatingSystemmapper);

            // Act & Assert
            var all = await service.GetAllAsync();
            Assert.Equal(2, all.Count());

            var os = await service.GetByIdAsync(Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"));
            Assert.Equal("OS1", os.Name);

            var newOs = await service.CreateAsync(new CreateOperatingSystemDto { Name = "OS3" });
            Assert.Equal("OS3", newOs.Name);

            var updated = await service.UpdateAsync(Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"), new UpdateOperatingSystemDto { Name = "UpdatedOS" });
            Assert.True(updated);

            var deleted = await service.DeleteAsync(Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"));
            Assert.True(deleted);




            // --- Duplicate ---
            await Assert.ThrowsAsync<DuplicateException>(() => service.CreateAsync(new CreateOperatingSystemDto { Name = "OS2" }));
            await Assert.ThrowsAsync<DuplicateException>(() => service.UpdateAsync(Guid.Parse("a3f5d2e7-8c4b-4a1f-9e2b-7d6f5c3b2a1e"), new UpdateOperatingSystemDto { Name = "OS3" }));

            // --- NotFound ---
            await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(Guid.NewGuid()));
            await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateAsync(Guid.NewGuid(), new UpdateOperatingSystemDto { Name = "X" }));
            await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(Guid.NewGuid()));
        }

        // ---------------- TAG ----------------
        [Fact]
        public async Task TagService_Crud_WithExceptions_Test()
        {
            // Arrange
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
            var tags = new List<Tag>
            {
                new Tag { Id = Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"), Name = "Tag1" },
                new Tag { Id = Guid.Parse("a3f5d2e7-8c4b-4a1f-9e2b-7d6f5c3b2a1e"), Name = "Tag2" }
            };
            var context = GetDbContext(tags);
            var service = new TagService(context, _tagmapper);

            // Act & Assert
            var all = await service.GetAllAsync();
            Assert.Equal(2, all.Count());

            var tag = await service.GetByIdAsync(Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"));
            Assert.Equal("Tag1", tag.Name);

            var newTag = await service.CreateAsync(new CreateTagDto { Name = "Tag3" });
            Assert.Equal("Tag3", newTag.Name);

            var updated = await service.UpdateAsync(Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"), new UpdateTagDto { Name = "UpdatedTag" });
            Assert.True(updated);

            var deleted = await service.DeleteAsync(Guid.Parse("d9b1f7c4-3f2a-4b6e-a2d1-8f5b7e9c2a1d"));
            Assert.True(deleted);




            // --- Duplicate ---
            await Assert.ThrowsAsync<DuplicateException>(() => service.CreateAsync(new CreateTagDto { Name = "Tag2" }));
            await Assert.ThrowsAsync<DuplicateException>(() => service.UpdateAsync(Guid.Parse("a3f5d2e7-8c4b-4a1f-9e2b-7d6f5c3b2a1e"), new UpdateTagDto { Name = "Tag3" }));

            // --- NotFound ---
            await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(Guid.NewGuid()));
            await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateAsync(Guid.NewGuid(), new UpdateTagDto { Name = "X" }));
            await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(Guid.NewGuid()));
        }
    }
}
