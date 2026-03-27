using Arzenal.Dto.DTOs.LanguageDto;
using Arzenal.Dto.Validators.LanguageDtoValidators;
using FluentValidation.TestHelper;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Validators.LanguagesValidatorTests
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
            // Arrange
            var model = new UpdateLanguageDto { Name = name };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Name_Is_Valid()
        {
            // Arrange
            var model = new UpdateLanguageDto { Name = "Spanish" };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }
    }
}
