using ArzenalStoreSharedDto.DTOs.AppDto;
using ArzenalStoreSharedDto.Validators.AppDtoValidators;
using FluentValidation.TestHelper;

namespace TestArzenalStoreApi.Unit.Validators.AppsValidatorTests
{
    public class CreateAppDtoValidatorTests
    {
        private readonly CreateAppDtoValidator _validator = new CreateAppDtoValidator();

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("", "", "")]
        [InlineData(" ", " ", " ")]
        public void Should_Have_Error_When_Name_Or_Version_Or_FilePath_Are_Invalid(string name, string version, string filepath)
        {
            var model = new CreateAppDto { Name = name, Version = version, FilePath = filepath };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Name);
            result.ShouldHaveValidationErrorFor(x => x.Version);
            result.ShouldHaveValidationErrorFor(x => x.FilePath);
        }

        [Fact]
        public void Should_Have_Error_When_Name_Or_Version_Or_FilePath_Are_Valid()
        {
            var model = new CreateAppDto { Name = "New App", Version = "1.0.0", FilePath = "C:/" };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
            result.ShouldNotHaveValidationErrorFor(x => x.Version);
            result.ShouldNotHaveValidationErrorFor(x => x.FilePath);
        }
    }
}
