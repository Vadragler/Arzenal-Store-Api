using Arzenal.Dto.DTOs.AppDto;
using Arzenal.Dto.Validators.AppDtoValidators;
using FluentValidation.TestHelper;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Validators.AppsValidatorTests
{
    public class CreateAppDtoValidatorTests
    {
        private readonly CreateAppDtoValidator _validator = new CreateAppDtoValidator();

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData(" ", " ")]
        public void Should_Have_Error_When_Name_Or_Version_Or_FilePath_Are_Invalid(string name, string version)
        {
            // Arrange
            var model = new CreateAppDto { Name = name, Version = version};

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name);
            result.ShouldHaveValidationErrorFor(x => x.Version);
        }

        [Fact]
        public void Should_Have_Error_When_Name_Or_Version_Or_FilePath_Are_Valid()
        {
            // Arrange
            var model = new CreateAppDto { Name = "New App", Version = "1.0.0" };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
            result.ShouldNotHaveValidationErrorFor(x => x.Version);
            
        }
    }
}
