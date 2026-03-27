using Arzenal.Dto.DTOs.AppDto;
using Arzenal.Dto.Validators.AppDtoValidators;
using FluentValidation.TestHelper;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Validators.AppsValidatorTests
{
    public class UpdateAppDtoValidatorTests
    {
        private readonly UpdateAppDtoValidator _validator = new UpdateAppDtoValidator();

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void Should_Have_Error_When_Version_Is_Invalid(string version)
        {
            // Arrange
            var model = new UpdateAppDto { Version = version };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Version);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Version_Is_Valid()
        {
            // Arrange
            var model = new UpdateAppDto { Version = "2.1.0" };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Version);
        }
    }
}
