using Arzenal.Dto.DTOs.FileDto;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Interfaces;
using ArzenalStoreApi.Controllers.Apps;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Controllers.AppFilesControllerTests
{
    public class AppFilesErrorTests
    {
        private readonly Mock<IAppFilesService> _serviceMock;
        private readonly AppFilesController _controller;

        public AppFilesErrorTests()
        {
            _serviceMock = new Mock<IAppFilesService>();
            _controller = new AppFilesController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetFilePath_NotFound_Throws()
        {
            // Arrange
            var appId = Guid.NewGuid();

            _serviceMock
                .Setup(s => s.GetFilePath(appId))
                .ThrowsAsync(new NotFoundException("Fichier introuvable."));

            // Act
            var result = await Assert.ThrowsAsync<NotFoundException>(
                () => _controller.GetFilePath(appId)
            );

            // Assert
            Assert.Equal("Fichier introuvable.", result.Message);
        }


        [Fact]
        public async Task DownloadApp_NotFound_Returns404()
        {
            var appId = Guid.NewGuid();

            _serviceMock
                .Setup(s => s.Download(appId))
                .ThrowsAsync(new NotFoundException("Aucune application trouvée."));

            var result = await Assert.ThrowsAsync<NotFoundException>(
                () => _controller.DownloadApp(appId)
            );

            Assert.Equal("Aucune application trouvée.", result.Message);
        }



        [Fact]
        public async Task UploadFile_InvalidStream_ThrowsValidationException()
        {
            // Arrange
            var appId = Guid.NewGuid();

            // DTO minimal
            var uploadFileDto = new UploadFileDto
            {
                AppId = appId,
                Type = "app",
                Version = "1.0.0",
                Platform = "android",
                FileName = "app.exe"
            };
            var context = new DefaultHttpContext();
            var emptyStream = new MemoryStream(); // vide mais lisible
            context.Request.Body = emptyStream;

            var serviceMock = new Mock<IAppFilesService>();
            serviceMock
                .Setup(s => s.UploadFile(
                    It.IsAny<UploadFileDto>(),
                    It.Is<Stream>(s => s.Length == 0))) // on teste la longueur plutôt que CanRead
                .ThrowsAsync(new ValidationException("Flux invalide."));


            var controller = new AppFilesController(serviceMock.Object);
            // Mock HttpContext et Request.Body

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(
                () => controller.UploadFile(uploadFileDto.AppId, uploadFileDto.Type, uploadFileDto.Version, uploadFileDto.Platform)
            );

            Assert.Equal("Flux invalide.", ex.Message);
        }




        [Fact]
        public async Task DeleteFile_NotFound_Throws()
        {
            // Arrange
            var appId = Guid.NewGuid();

            _serviceMock
                .Setup(s => s.DeleteFile(appId))
                .ThrowsAsync(new NotFoundException("Aucune application trouvée."));

            // Act
            var result = await Assert.ThrowsAsync<NotFoundException>(
                () => _controller.DeleteFile(appId)
            );

            // Assert
            Assert.Equal("Aucune application trouvée.", result.Message);
        }



    }
}
