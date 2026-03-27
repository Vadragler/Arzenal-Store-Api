using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Dto.DTOs.CategorieDto;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ArzenalStoreApi.Controllers.Apps;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Controllers.CategoriesControllerTests
{
    public class CategoriesSuccessTests
    {
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly CategoriesController _controller;

        public CategoriesSuccessTests()
        {
            _categoryServiceMock = new Mock<ICategoryService>();
            _controller = new CategoriesController(_categoryServiceMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithList()
        {
            // Arrange
            var categories = new List<ReadCategorieDto>
            {
                new ReadCategorieDto { Id = Guid.NewGuid(), Name = "Cat1" },
                new ReadCategorieDto { Id = Guid.NewGuid(), Name = "Cat2" }
            };
            _categoryServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(categories);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<List<ReadCategorieDto>>(okResult.Value);
            Assert.Equal(2, data.Count);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WithCategory()
        {
            // Arrange
            var id = Guid.NewGuid();
            var category = new ReadCategorieDto { Id = id, Name = "TestCat" };
            _categoryServiceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(category);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<ReadCategorieDto>(okResult.Value);
            Assert.Equal(id, data.Id);
        }

        [Fact]
        public async Task PostCategorie_ReturnsCreatedAtAction()
        {
            // Arrange
            var createDto = new CreateCategorieDto { Name = "NewCat" };
            var readDto = new ReadCategorieDto { Id = Guid.NewGuid(), Name = "NewCat" };
            _categoryServiceMock.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(readDto);

            // Act
            var result = await _controller.PostCategorie(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var data = Assert.IsType<ReadCategorieDto>(createdResult.Value);
            Assert.Equal(readDto.Id, data.Id);
        }

        [Fact]
        public async Task PutCategorie_ReturnsNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            var updateDto = new UpdateCategorieDto { Name = "Updated" };
            _categoryServiceMock.Setup(s => s.UpdateAsync(id, updateDto)).ReturnsAsync(true);

            // Act
            var result = await _controller.PutCategorie(id, updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteCategorie_ReturnsNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            _categoryServiceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteCategorie(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
