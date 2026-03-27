using Arzenal.Dto.DTOs.TagDto;
using Arzenal.Store.Api.Service.Interfaces;
using ArzenalStoreApi.Controllers.Apps;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Controllers.TagsControllerTests
{
    public class TagsSuccessTests
    {
        private readonly Mock<ITagService> _tagServiceMock;
        private readonly TagsController _controller;

        public TagsSuccessTests()
        {
            _tagServiceMock = new Mock<ITagService>();
            _controller = new TagsController(_tagServiceMock.Object);
        }

        [Fact]
        public async Task GetAllTag_ReturnsOk_WithList()
        {
            // Arrange
            var tags = new List<ReadTagDto>
            {
                new ReadTagDto { Id = Guid.NewGuid(), Name = "Action" },
                new ReadTagDto { Id = Guid.NewGuid(), Name = "RPG" }
            };
            _tagServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(tags);

            // Act
            var result = await _controller.GetAllTags();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<List<ReadTagDto>>(okResult.Value);
            Assert.Equal(2, data.Count);
        }

        [Fact]
        public async Task GetTagById_ReturnsOk()
        {
            // Arrange
            var id = Guid.NewGuid();
            var tag = new ReadTagDto { Id = id, Name = "Aventure" };
            _tagServiceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(tag);

            // Act
            var result = await _controller.GetTagById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<ReadTagDto>(okResult.Value);
            Assert.Equal(id, data.Id);
        }

        [Fact]
        public async Task PostTag_ReturnsCreatedAtAction()
        {
            // Arrange
            var createDto = new CreateTagDto { Name = "Indie" };
            var readDto = new ReadTagDto { Id = Guid.NewGuid(), Name = "Indie" };
            _tagServiceMock.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(readDto);

            // Act
            var result = await _controller.PostTag(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var data = Assert.IsType<ReadTagDto>(createdResult.Value);
            Assert.Equal(readDto.Id, data.Id);
        }

        [Fact]
        public async Task PutTag_ReturnsNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            var updateDto = new UpdateTagDto { Name = "MMORPG" };
            _tagServiceMock.Setup(s => s.UpdateAsync(id, updateDto)).ReturnsAsync(true);

            // Act
            var result = await _controller.PutTag(id, updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteTag_ReturnsNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            _tagServiceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteTag(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
