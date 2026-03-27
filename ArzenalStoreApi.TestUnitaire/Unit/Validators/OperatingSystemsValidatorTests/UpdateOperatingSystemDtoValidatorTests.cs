using Arzenal.Dto.DTOs.OperatingSystemDto;
using Arzenal.Dto.Validators.OperatingSystemDtoValidators;
using FluentValidation.TestHelper;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Validators.OperatingSystemsValidatorTests
{
    public class UpdateOperatingSystemDtoValidatorTests
    {
        private readonly UpdateOperatingSystemDtoValidator _validator = new UpdateOperatingSystemDtoValidator();

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
            var model = new UpdateOperatingSystemDto { Name = name };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Name_Is_Valid()
        {
            // Arrange
            var model = new UpdateOperatingSystemDto { Name = "Linux" };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }
    }
}
