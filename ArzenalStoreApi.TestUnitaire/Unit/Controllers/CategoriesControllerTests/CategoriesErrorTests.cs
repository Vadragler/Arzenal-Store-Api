using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Dto.DTOs.CategorieDto;
using Moq;
using ArzenalStoreApi.Controllers.Apps;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Controllers.CategoriesControllerTests
{
    public class CategoriesErrorTests
    {
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly CategoriesController _controller;

        public CategoriesErrorTests()
        {
            _categoryServiceMock = new Mock<ICategoryService>();
            _controller = new CategoriesController(_categoryServiceMock.Object);
        }

        [Fact]
        public async Task GetAll_ThrowsException()
        {
            // Arrange
            _categoryServiceMock.Setup(s => s.GetAllAsync())
                .ThrowsAsync(new Exception("Erreur récupération"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.GetAll());
            Assert.Equal("Erreur récupération", ex.Message);
        }

        [Fact]
        public async Task GetById_ThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _categoryServiceMock.Setup(s => s.GetByIdAsync(id))
                .ThrowsAsync(new Exception("Catégorie introuvable"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.GetById(id));
            Assert.Equal("Catégorie introuvable", ex.Message);
        }

        [Fact]
        public async Task PostCategorie_ThrowsException()
        {
            // Arrange
            var createDto = new CreateCategorieDto { Name = "DuplicateCat" };
            _categoryServiceMock.Setup(s => s.CreateAsync(createDto))
                .ThrowsAsync(new Exception("Catégorie existe déjà"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.PostCategorie(createDto));
            Assert.Equal("Catégorie existe déjà", ex.Message);
        }

        [Fact]
        public async Task PutCategorie_ThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var updateDto = new UpdateCategorieDto { Name = "Updated" };
            _categoryServiceMock.Setup(s => s.UpdateAsync(id, updateDto))
                .ThrowsAsync(new Exception("Erreur update"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.PutCategorie(id, updateDto));
            Assert.Equal("Erreur update", ex.Message);
        }

        [Fact]
        public async Task DeleteCategorie_ThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _categoryServiceMock.Setup(s => s.DeleteAsync(id))
                .ThrowsAsync(new Exception("Erreur suppression"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.DeleteCategorie(id));
            Assert.Equal("Erreur suppression", ex.Message);
        }
    }
}
