using Arzenal.Dto.DTOs.AppFileDto;
using Arzenal.Dto.DTOs.FileDto;
using Arzenal.Store.Api.Service.Interfaces;
using ArzenalStoreApi.Controllers.Apps;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Controllers.AppFilesControllerTests
{
    public class AppFilesSuccessTests
    {
        private readonly Mock<IAppFilesService> _serviceMock;
        private readonly AppFilesController _controller;

        public AppFilesSuccessTests()
        {
            _serviceMock = new Mock<IAppFilesService>();
            _controller = new AppFilesController(_serviceMock.Object); // DbContext non utilisé ici
        }

        [Fact]
        public async Task GetFilePath_ReturnsOkWithPath()
        {
            // Arrange
            var appId = Guid.NewGuid();
            var fakePath = "/fake/path/file.txt";

            _serviceMock.Setup(s => s.GetFilePath(appId))
                        .ReturnsAsync(fakePath);

            // Act
            var result = await _controller.GetFilePath(appId) as OkObjectResult;

            // Assert
            Assert.NotNull(result);

            
            var data = Assert.IsType<ReadAppFilePathDto>(result.Value);
            Assert.Equal(appId, data.Id);
            Assert.Equal(fakePath, data.FilePath);

        }

        [Fact]
        public async Task DownloadApp_ReturnsFile()
        {
            // Arrange
            var appId = Guid.NewGuid();
            var content = new byte[] { 1, 2, 3 };
            var stream = new MemoryStream(content);

            _serviceMock
                .Setup(s => s.Download(appId))
                .ReturnsAsync(new AppFileStreamResult(
                    stream,
                    "app.exe",
                    "application/octet-stream"
                ));

            // Act
            var result = await _controller.DownloadApp(appId);

            // Assert
            var fileResult = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("application/octet-stream", fileResult.ContentType);
            Assert.Equal("app.exe", fileResult.FileDownloadName);
        }


        [Fact]
        public async Task UploadFile_ReturnsCreatedAtAction()
        {
            // Arrange
            var appId = Guid.NewGuid();
            var pathReturned = "/fake/path/uploaded.txt";

            var uploadFileDto = new UploadFileDto
            {
                AppId = appId,
                Type = "app",
                Version = "1.0.0"
            };

            // Mock service pour renvoyer un chemin au lieu de lancer une exception
            _serviceMock.Setup(s => s.UploadFile(It.IsAny<UploadFileDto>(), It.IsAny<Stream>()))
                .ReturnsAsync(pathReturned);

            // Création du controller avec HttpContext et Request.Body valide
            var controller = new AppFilesController(_serviceMock.Object);
            var context = new DefaultHttpContext();
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("fake content"));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            // Act
            var result = await controller.UploadFile(uploadFileDto.AppId, "1.0.0", "app","app.txt")
                         as CreatedAtActionResult;

            // Assert
            Assert.NotNull(result);
            var data = Assert.IsType<ReadAppFilePathDto>(result.Value);
            Assert.Equal(appId, data.Id);
            Assert.Equal(pathReturned, data.FilePath);
        }


        [Fact]
        public async Task DeleteFile_ReturnsNoContent()
        {
            var appId = Guid.NewGuid();
            _serviceMock.Setup(s => s.DeleteFile(appId)).ReturnsAsync(true);

            var result = await _controller.DeleteFile(appId);

            Assert.IsType<NoContentResult>(result);
        }
    }
}
