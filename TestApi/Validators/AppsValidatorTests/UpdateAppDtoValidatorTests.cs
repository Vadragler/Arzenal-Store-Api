using Arzenal.Shared.Dtos.DTOs.AppDto;
using Arzenal.Shared.Dtos.Validators.AppDtoValidators;
using FluentValidation.TestHelper;

namespace TestApi.Validators.AppsValidatorTests
{
    public class UpdateAppDtoValidatorTests
    {
        private readonly UpdateAppDtoValidator _validator = new UpdateAppDtoValidator();

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void Should_Have_Error_When_Version_Is_Invalid(string version)
        {
            var model = new UpdateAppDto { Version = version };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Version);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Version_Is_Valid()
        {
            var model = new UpdateAppDto { Version = "2.1.0" };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Version);
        }
    }
}
