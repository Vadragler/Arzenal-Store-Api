using Arzenal.Shared.Dtos.DTOs.TagDto;
using Arzenal.Shared.Dtos.Validators.TagDtoValidators;
using FluentValidation.TestHelper;

namespace TestApi.Validators.TagsValidatorTests
{
    public class CreateTagDtoValidatorTests
    {
        private readonly CreateTagDtoValidator _validator = new CreateTagDtoValidator();

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
            var model = new CreateTagDto { Name = name };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Name_Is_Valid()
        {
            var model = new CreateTagDto { Name = "Action" };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }
    }
}
