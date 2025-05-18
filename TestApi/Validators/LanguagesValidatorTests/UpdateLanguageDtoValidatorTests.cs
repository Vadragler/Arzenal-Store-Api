using ArzenalApi.DTOs.LanguageDto;
using ArzenalApi.Validators.LanguageDtoValidators;
using FluentValidation.TestHelper;

namespace TestApi.Validators.LanguagesValidatorTests
{
    public class UpdateLanguageDtoValidatorTests
    {
        private readonly UpdateLanguageDtoValidator _validator = new UpdateLanguageDtoValidator();

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
            var model = new UpdateLanguageDto { Name = name };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Name_Is_Valid()
        {
            var model = new UpdateLanguageDto { Name = "Spanish" };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }
    }
}
