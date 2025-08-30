using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ArzenalStoreApi.Data;
using ArzenalStoreApi.Models;
using ArzenalStoreApi.Services.CategoryService;
using ArzenalStoreApi.Services.LanguageService;
using ArzenalStoreApi.Services.OperatingSystemService;
using ArzenalStoreApi.Services.TagService;
using ArzenalStoreApi.Exceptions;
using ArzenalStoreSharedDto.DTOs.CategorieDto;
using ArzenalStoreSharedDto.DTOs.LanguageDto;
using ArzenalStoreSharedDto.DTOs.OperatingSystemDto;
using ArzenalStoreSharedDto.DTOs.TagDto;
using OperatingSystem = ArzenalStoreApi.Models.OperatingSystem;

namespace TestArzenalStoreApi.Unit.Services
{
    public class CrudServicesTests
    {
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
            var categories = new List<Categorie>
            {
                new Categorie { Id = 1, Name = "Cat1" },
                new Categorie { Id = 2, Name = "Cat2" }
            };
            var context = GetDbContext(categories);
            var service = new CategoryService(context);

            // --- Happy Path ---
            var all = await service.GetAllAsync();
            Assert.Equal(2, all.Count());

            var cat = await service.GetByIdAsync(1);
            Assert.Equal("Cat1", cat.Name);

            var newCat = await service.CreateAsync(new CreateCategorieDto { Name = "Cat3" });
            Assert.Equal("Cat3", newCat.Name);

            var updated = await service.UpdateAsync(1, new UpdateCategorieDto { Name = "UpdatedCat" });
            Assert.True(updated);

            var deleted = await service.DeleteAsync(1);
            Assert.True(deleted);

            // --- Validation Exceptions ---
            await Assert.ThrowsAsync<ValidationException>(() => service.GetByIdAsync(0));
            await Assert.ThrowsAsync<ValidationException>(() => service.UpdateAsync(0, new UpdateCategorieDto { Name = "X" }));
            await Assert.ThrowsAsync<ValidationException>(() => service.DeleteAsync(0));


            // --- Duplicate Exceptions ---
            await Assert.ThrowsAsync<DuplicateException>(() => service.CreateAsync(new CreateCategorieDto { Name = "Cat2" }));
            await Assert.ThrowsAsync<DuplicateException>(() => service.UpdateAsync(2, new UpdateCategorieDto { Name = "Cat2" }));

            // --- NotFound Exceptions ---
            await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(99));
            await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateAsync(99, new UpdateCategorieDto { Name = "X" }));
            await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(99));
        }

        // ---------------- LANGUAGE ----------------
        [Fact]
        public async Task LanguageService_Crud_WithExceptions_Test()
        {
            var languages = new List<Language>
            {
                new Language { Id = 1, Name = "Lang1" },
                new Language { Id = 2, Name = "Lang2" }
            };
            var context = GetDbContext(languages);
            var service = new LanguageService(context);

            var all = await service.GetAllAsync();
            Assert.Equal(2, all.Count());

            var lang = await service.GetByIdAsync(1);
            Assert.Equal("Lang1", lang.Name);

            var newLang = await service.CreateAsync(new CreateLanguageDto { Name = "Lang3" });
            Assert.Equal("Lang3", newLang.Name);

            var updated = await service.UpdateAsync(1, new UpdateLanguageDto { Name = "UpdatedLang" });
            Assert.True(updated);

            var deleted = await service.DeleteAsync(1);
            Assert.True(deleted);

            // --- Validation ---
            await Assert.ThrowsAsync<ValidationException>(() => service.GetByIdAsync(0));
            await Assert.ThrowsAsync<ValidationException>(() => service.UpdateAsync(0, new UpdateLanguageDto { Name = "X" }));
            await Assert.ThrowsAsync<ValidationException>(() => service.DeleteAsync(0));


            // --- Duplicate ---
            await Assert.ThrowsAsync<DuplicateException>(() => service.CreateAsync(new CreateLanguageDto { Name = "Lang2" }));
            await Assert.ThrowsAsync<DuplicateException>(() => service.UpdateAsync(3, new UpdateLanguageDto { Name = "Lang2" }));

            // --- NotFound ---
            await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(99));
            await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateAsync(99, new UpdateLanguageDto { Name = "X" }));
            await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(99));
        }

        // ---------------- OPERATING SYSTEM ----------------
        [Fact]
        public async Task OperatingSystemService_Crud_WithExceptions_Test()
        {
            var oss = new List<OperatingSystem>
            {
                new OperatingSystem { Id = 1, Name = "OS1" },
                new OperatingSystem { Id = 2, Name = "OS2" }
            };
            var context = GetDbContext(oss);
            var service = new OperatingSystemService(context);

            var all = await service.GetAllAsync();
            Assert.Equal(2, all.Count());

            var os = await service.GetByIdAsync(1);
            Assert.Equal("OS1", os.Name);

            var newOs = await service.CreateAsync(new CreateOperatingSystemDto { Name = "OS3" });
            Assert.Equal("OS3", newOs.Name);

            var updated = await service.UpdateAsync(1, new UpdateOperatingSystemDto { Name = "UpdatedOS" });
            Assert.Equal("UpdatedOS", updated.Name);

            var deleted = await service.DeleteAsync(1);
            Assert.True(deleted);

            // --- Validation ---
            await Assert.ThrowsAsync<ValidationException>(() => service.GetByIdAsync(0));
            await Assert.ThrowsAsync<ValidationException>(() => service.UpdateAsync(0, new UpdateOperatingSystemDto { Name = "X" }));
            await Assert.ThrowsAsync<ValidationException>(() => service.DeleteAsync(0));


            // --- Duplicate ---
            await Assert.ThrowsAsync<DuplicateException>(() => service.CreateAsync(new CreateOperatingSystemDto { Name = "OS2" }));
            await Assert.ThrowsAsync<DuplicateException>(() => service.UpdateAsync(3, new UpdateOperatingSystemDto { Name = "OS2" }));

            // --- NotFound ---
            await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(99));
            await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateAsync(99, new UpdateOperatingSystemDto { Name = "X" }));
            await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(99));
        }

        // ---------------- TAG ----------------
        [Fact]
        public async Task TagService_Crud_WithExceptions_Test()
        {
            var tags = new List<Tag>
            {
                new Tag { Id = 1, Name = "Tag1" },
                new Tag { Id = 2, Name = "Tag2" }
            };
            var context = GetDbContext(tags);
            var service = new TagService(context);

            var all = await service.GetAllAsync();
            Assert.Equal(2, all.Count());

            var tag = await service.GetByIdAsync(1);
            Assert.Equal("Tag1", tag.Name);

            var newTag = await service.CreateAsync(new CreateTagDto { Name = "Tag3" });
            Assert.Equal("Tag3", newTag.Name);

            var updated = await service.UpdateAsync(1, new UpdateTagDto { Name = "UpdatedTag" });
            Assert.Equal("UpdatedTag", updated.Name);

            var deleted = await service.DeleteAsync(1);
            Assert.True(deleted);

            // --- Validation ---
            await Assert.ThrowsAsync<ValidationException>(() => service.GetByIdAsync(0));
            await Assert.ThrowsAsync<ValidationException>(() => service.UpdateAsync(0, new UpdateTagDto { Name = "X" }));
            await Assert.ThrowsAsync<ValidationException>(() => service.DeleteAsync(0));


            // --- Duplicate ---
            await Assert.ThrowsAsync<DuplicateException>(() => service.CreateAsync(new CreateTagDto { Name = "Tag2" }));
            await Assert.ThrowsAsync<DuplicateException>(() => service.UpdateAsync(3, new UpdateTagDto { Name = "Tag2" }));

            // --- NotFound ---
            await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(99));
            await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateAsync(99, new UpdateTagDto { Name = "X" }));
            await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(99));
        }
    }
}
