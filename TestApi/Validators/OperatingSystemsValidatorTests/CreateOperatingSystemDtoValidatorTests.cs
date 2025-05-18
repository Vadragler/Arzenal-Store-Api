using Arzenal.Shared.Dtos.DTOs.OperatingSystemDto;
using Arzenal.Shared.Dtos.Validators.OperatingSystemDtoValidators;
using FluentValidation.TestHelper;

namespace TestApi.Validators.OperatingSystemsValidatorTests
{
    public class CreateOperatingSystemDtoValidatorTests
    {
        private readonly CreateOperatingSystemDtoValidator _validator = new CreateOperatingSystemDtoValidator();

        public static IEnumerable<object[]> LongStrings =>
        new List<object[]>
        {
            new object[] { new string('A', 101) },
            new object[] { "" },
            new object[] { null! },
            new object[] { " " }
        };

        [Theory]
        [MemberData(nameof(LongStrings))]
        public void Should_Have_Error_When_Name_Is_Invalid(string name)
        {
            var model = new CreateOperatingSystemDto { Name = name };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Name_Is_Valid()
        {
            var model = new CreateOperatingSystemDto { Name = "Windows" };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }
    }
}
