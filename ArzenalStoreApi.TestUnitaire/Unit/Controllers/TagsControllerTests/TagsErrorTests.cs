using Arzenal.Dto.DTOs.TagDto;
using Arzenal.Store.Api.Service.Interfaces;
using ArzenalStoreApi.Controllers.Apps;
using Moq;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Controllers.TagsControllerTests
{
    public class TagsErrorTests
    {
        private readonly Mock<ITagService> _tagServiceMock;
        private readonly TagsController _controller;

        public TagsErrorTests()
        {
            _tagServiceMock = new Mock<ITagService>();
            _controller = new TagsController(_tagServiceMock.Object);
        }

        [Fact]
        public async Task GetAllTag_ThrowsException()
        {
            // Arrange
            _tagServiceMock.Setup(s => s.GetAllAsync())
                .ThrowsAsync(new Exception("Tag introuvable"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.GetAllTags());
            Assert.Equal("Tag introuvable", ex.Message);
        }

        [Fact]
        public async Task GetTagById_ThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _tagServiceMock.Setup(s => s.GetByIdAsync(id))
                .ThrowsAsync(new Exception("Tag introuvable"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.GetTagById(id));
            Assert.Equal("Tag introuvable", ex.Message);
        }

        [Fact]
        public async Task PostTag_ThrowsException()
        {
            // Arrange
            var createDto = new CreateTagDto { Name = "Duplicate" };
            _tagServiceMock.Setup(s => s.CreateAsync(createDto))
                .ThrowsAsync(new Exception("Ce Tag existe déjà"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.PostTag(createDto));
            Assert.Equal("Ce Tag existe déjà", ex.Message);
        }

        [Fact]
        public async Task PutTag_ThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var updateDto = new UpdateTagDto { Name = "ErreurUpdate" };
            _tagServiceMock.Setup(s => s.UpdateAsync(id, updateDto))
                .ThrowsAsync(new Exception("Erreur update"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.PutTag(id, updateDto));
            Assert.Equal("Erreur update", ex.Message);
        }

        [Fact]
        public async Task DeleteTag_ThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _tagServiceMock.Setup(s => s.DeleteAsync(id))
                .ThrowsAsync(new Exception("Erreur suppression"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.DeleteTag(id));
            Assert.Equal("Erreur suppression", ex.Message);
        }
    }
}
