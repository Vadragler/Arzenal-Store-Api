using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Dto.DTOs.LanguageDto;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ArzenalStoreApi.Controllers.Apps;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Controllers.LanguagesControllerTests
{
    public class LanguagesSuccessTests
    {
        private readonly Mock<ILanguageService> _languageServiceMock;
        private readonly LanguagesController _controller;

        public LanguagesSuccessTests()
        {
            _languageServiceMock = new Mock<ILanguageService>();
            _controller = new LanguagesController(_languageServiceMock.Object);
        }

        [Fact]
        public async Task GetAllLanguages_ReturnsOk_WithList()
        {
            // Arrange
            var languages = new List<ReadLanguageDto>
            {
                new ReadLanguageDto { Id = Guid.NewGuid(), Name = "French" },
                new ReadLanguageDto { Id = Guid.NewGuid(), Name = "English" }
            };

            _languageServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(languages);

            // Act
            var result = await _controller.GetAllLanguages();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<List<ReadLanguageDto>>(okResult.Value);
            Assert.Equal(2, data.Count);
        }

        [Fact]
        public async Task GetLanguageById_ReturnsOk_WithLanguage()
        {
            // Arrange
            var id = Guid.NewGuid();
            var language = new ReadLanguageDto { Id = id, Name = "Spanish" };
            _languageServiceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(language);

            // Act
            var result = await _controller.GetLanguageById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<ReadLanguageDto>(okResult.Value);
            Assert.Equal(id, data.Id);
        }

        [Fact]
        public async Task PostLanguage_ReturnsCreatedAtAction()
        {
            // Arrange
            var createDto = new CreateLanguageDto { Name = "German" };
            var readDto = new ReadLanguageDto { Id = Guid.NewGuid(), Name = "German" };
            _languageServiceMock.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(readDto);

            // Act
            var result = await _controller.PostLanguage(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var data = Assert.IsType<ReadLanguageDto>(createdResult.Value);
            Assert.Equal(readDto.Id, data.Id);
        }

        [Fact]
        public async Task PutLanguage_ReturnsNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            var updateDto = new UpdateLanguageDto { Name = "Italian" };
            _languageServiceMock.Setup(s => s.UpdateAsync(id, updateDto)).ReturnsAsync(true);

            // Act
            var result = await _controller.PutLanguage(id, updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteLanguage_ReturnsNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            _languageServiceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteLanguage(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
