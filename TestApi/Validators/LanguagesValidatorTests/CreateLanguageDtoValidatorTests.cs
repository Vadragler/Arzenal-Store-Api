using ArzenalApi.DTOs.LanguageDto;
using ArzenalApi.Validators.LanguageDtoValidators;
using FluentValidation.TestHelper;

namespace TestApi.Validators.LanguagesValidatorTests
{
    public class CreateLanguageDtoValidatorTests
    {
        private readonly CreateLanguageDtoValidator _validator = new CreateLanguageDtoValidator();

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
            var model = new CreateLanguageDto { Name = name };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Name_Is_Valid()
        {
            var model = new CreateLanguageDto { Name = "French" };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }
    }
}
