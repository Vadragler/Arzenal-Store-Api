using ArzenalApi.DTOs.CategorieDto;
using ArzenalApi.Validators.CategorieDtoValidators;
using FluentValidation.TestHelper;

namespace TestApi.Validators.CategoriesValidatorTests
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
            var model = new CreateCategorieDto { Name = name };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Name_Is_Valid()
        {
            var model = new CreateCategorieDto { Name = "Utilities" };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }
    }
}
