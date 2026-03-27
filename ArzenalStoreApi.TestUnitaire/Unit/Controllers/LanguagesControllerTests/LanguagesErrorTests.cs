using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Dto.DTOs.LanguageDto;
using Moq;
using ArzenalStoreApi.Controllers.Apps;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Controllers.LanguagesControllerTests
{
    public class LanguagesErrorTests
    {
        private readonly Mock<ILanguageService> _languageServiceMock;
        private readonly LanguagesController _controller;

        public LanguagesErrorTests()
        {
            _languageServiceMock = new Mock<ILanguageService>();
            _controller = new LanguagesController(_languageServiceMock.Object);
        }

        [Fact]
        public async Task GetAllLanguages_ThrowsException()
        {
            // Arrange
            _languageServiceMock.Setup(s => s.GetAllAsync())
                .ThrowsAsync(new Exception("Erreur récupération"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.GetAllLanguages());
            Assert.Equal("Erreur récupération", ex.Message);
        }

        [Fact]
        public async Task GetLanguageById_ThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _languageServiceMock.Setup(s => s.GetByIdAsync(id))
                .ThrowsAsync(new Exception("Language introuvable"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.GetLanguageById(id));
            Assert.Equal("Language introuvable", ex.Message);
        }

        [Fact]
        public async Task PostLanguage_ThrowsException()
        {
            // Arrange
            var createDto = new CreateLanguageDto { Name = "Duplicate" };
            _languageServiceMock.Setup(s => s.CreateAsync(createDto))
                .ThrowsAsync(new Exception("Cette langue existe déjà"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.PostLanguage(createDto));
            Assert.Equal("Cette langue existe déjà", ex.Message);
        }

        [Fact]
        public async Task PutLanguage_ThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var updateDto = new UpdateLanguageDto { Name = "Updated" };
            _languageServiceMock.Setup(s => s.UpdateAsync(id, updateDto))
                .ThrowsAsync(new Exception("Erreur update"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.PutLanguage(id, updateDto));
            Assert.Equal("Erreur update", ex.Message);
        }

        [Fact]
        public async Task DeleteLanguage_ThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _languageServiceMock.Setup(s => s.DeleteAsync(id))
                .ThrowsAsync(new Exception("Erreur suppression"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.DeleteLanguage(id));
            Assert.Equal("Erreur suppression", ex.Message);
        }
    }
}
