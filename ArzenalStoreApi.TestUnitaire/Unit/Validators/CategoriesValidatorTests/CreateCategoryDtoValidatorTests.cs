using Arzenal.Dto.DTOs.CategorieDto;
using Arzenal.Dto.Validators.CategorieDtoValidators;
using FluentValidation.TestHelper;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Validators.CategoriesValidatorTests
{
    public class CreateCategoryDtoValidatorTests
    {
        private readonly CreateCategorieDtoValidator _validator = new CreateCategorieDtoValidator();

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
            var model = new CreateCategorieDto { Name = name };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Name_Is_Valid()
        {
            // Arrange
            var model = new CreateCategorieDto { Name = "Utilities" };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }
    }
}
