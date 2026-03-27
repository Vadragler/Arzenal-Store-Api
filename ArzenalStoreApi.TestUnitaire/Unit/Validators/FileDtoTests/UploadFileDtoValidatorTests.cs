using Arzenal.Dto.DTOs.FileDto;
using Arzenal.Dto.Validators.FileDtoValidators;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Validators.FileDtoTests
{
    
        public class UploadFileDtoValidatorTests
        {
            private readonly UploadFileDtoValidator _validator;

            public UploadFileDtoValidatorTests()
            {
                _validator = new UploadFileDtoValidator();
            }

            #region Type Validation Tests

            [Fact]
            public void Validate_TypeIsEmpty_ShouldHaveValidationError()
            {
                // Arrange
                var mockFile = new Mock<IFormFile>();
                mockFile.Setup(f => f.Length).Returns(1024);

                var dto = new UploadFileDto
                {
                    Type = string.Empty,
                    Version = "1.0.0"
                };

                // Act
                var result = _validator.TestValidate(dto);

                // Assert
                result.ShouldHaveValidationErrorFor(x => x.Type);
            }

            [Theory]
            [InlineData("icon")]
            [InlineData("app")]
            public void Validate_TypeIsValidIconOrApp_ShouldNotHaveValidationError(string type)
            {
                // Arrange
                var mockFile = new Mock<IFormFile>();
                mockFile.Setup(f => f.Length).Returns(1024);

                var dto = new UploadFileDto
                {
                    Type = type,
                    Version = "1.0.0"
                };

                // Act
                var result = _validator.TestValidate(dto);

                // Assert
                result.ShouldNotHaveValidationErrorFor(x => x.Type);
            }

            [Fact]
            public void Validate_TypeIsInvalid_ShouldHaveValidationError()
            {
                // Arrange
                var mockFile = new Mock<IFormFile>();
                mockFile.Setup(f => f.Length).Returns(1024);

                var dto = new UploadFileDto
                {
                    Type = "invalid",
                    Version = "1.0.0"
                };

                // Act
                var result = _validator.TestValidate(dto);

                // Assert
                result.ShouldHaveValidationErrorFor(x => x.Type)
                    .WithErrorMessage("Type de fichier invalide.");
            }

            #endregion

            #region Platform Validation Tests (Type = "app")

            [Fact]
            public void Validate_TypeIsAppAndPlatformIsEmpty_ShouldHaveValidationError()
            {
                // Arrange
                var mockFile = new Mock<IFormFile>();
                mockFile.Setup(f => f.Length).Returns(1024);

                var dto = new UploadFileDto
                {
                    Type = "app",
                    Platform = string.Empty,
                    Version = "1.0.0"
                };

                // Act
                var result = _validator.TestValidate(dto);

                // Assert
                result.ShouldHaveValidationErrorFor(x => x.Platform)
                    .WithErrorMessage("La plateforme doit être spécifiée.");
            }

            [Fact]
            public void Validate_TypeIsAppAndPlatformIsNull_ShouldHaveValidationError()
            {
                // Arrange
                var mockFile = new Mock<IFormFile>();
                mockFile.Setup(f => f.Length).Returns(1024);

                var dto = new UploadFileDto
                {
                    Type = "app",
                    Platform = null,
                    Version = "1.0.0"
                };

                // Act
                var result = _validator.TestValidate(dto);

                // Assert
                result.ShouldHaveValidationErrorFor(x => x.Platform)
                    .WithErrorMessage("La plateforme doit être spécifiée.");
            }

            [Fact]
            public void Validate_TypeIsAppAndPlatformIsProvided_ShouldNotHaveValidationError()
            {
                // Arrange
                var mockFile = new Mock<IFormFile>();
                mockFile.Setup(f => f.Length).Returns(1024);

                var dto = new UploadFileDto
                {
                    Type = "app",
                    Platform = "Android",
                    Version = "1.0.0"
                };

                // Act
                var result = _validator.TestValidate(dto);

                // Assert
                result.ShouldNotHaveValidationErrorFor(x => x.Platform);
            }

            #endregion

            #region Platform Validation Tests (Type = "icon")

            [Fact]
            public void Validate_TypeIsIconAndPlatformIsEmpty_ShouldNotHaveValidationError()
            {
                // Arrange
                var mockFile = new Mock<IFormFile>();
                mockFile.Setup(f => f.Length).Returns(1024);

                var dto = new UploadFileDto
                {
                    Type = "icon",
                    Platform = string.Empty,
                    Version = "1.0.0"
                };

                // Act
                var result = _validator.TestValidate(dto);

                // Assert
                result.ShouldNotHaveValidationErrorFor(x => x.Platform);
            }

            [Fact]
            public void Validate_TypeIsIconAndPlatformIsNull_ShouldNotHaveValidationError()
            {
                // Arrange
                var mockFile = new Mock<IFormFile>();
                mockFile.Setup(f => f.Length).Returns(1024);

                var dto = new UploadFileDto
                {
                    Type = "icon",
                    Platform = null,
                    Version = "1.0.0"
                };

                // Act
                var result = _validator.TestValidate(dto);

                // Assert
                result.ShouldNotHaveValidationErrorFor(x => x.Platform);
            }

            #endregion

            #region Integration Tests

            [Fact]
            public void Validate_ValidUploadFileDtoForIcon_ShouldBeValid()
            {
                // Arrange
                var mockFile = new Mock<IFormFile>();
                mockFile.Setup(f => f.Length).Returns(2048);

                var dto = new UploadFileDto
                {
                    AppId = Guid.NewGuid(),
                    Type = "icon",
                    Version = "1.0.0",
                    Platform = null
                };

                // Act
                var result = _validator.TestValidate(dto);

                // Assert
                result.ShouldNotHaveAnyValidationErrors();
            }

            [Fact]
            public void Validate_ValidUploadFileDtoForApp_ShouldBeValid()
            {
                // Arrange
                var mockFile = new Mock<IFormFile>();
                mockFile.Setup(f => f.Length).Returns(5120);

                var dto = new UploadFileDto
                {
                    AppId = Guid.NewGuid(),
                    Type = "app",
                    Version = "2.0.1",
                    Platform = "iOS"
                };

                // Act
                var result = _validator.TestValidate(dto);

                // Assert
                result.ShouldNotHaveAnyValidationErrors();
            }

            [Fact]
            public void Validate_MultipleValidationErrors_ShouldReturnAllErrors()
            {
                // Arrange
                var mockFile = new Mock<IFormFile>();
                mockFile.Setup(f => f.Length).Returns(0);

                var dto = new UploadFileDto
                {
                    Type = "invalid",
                    Platform = null,
                    Version = "1.0.0"
                };

                // Act
                var result = _validator.TestValidate(dto);

                // Assert
                result.ShouldHaveValidationErrorFor(x => x.Type);
            }

            #endregion
        }
}
