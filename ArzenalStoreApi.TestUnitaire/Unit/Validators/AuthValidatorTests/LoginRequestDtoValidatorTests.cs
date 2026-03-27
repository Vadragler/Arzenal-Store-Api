using Arzenal.Dto.DTOs.AuthDto;
using Arzenal.Dto.Validators.AuthDtoValidators;
using FluentValidation.TestHelper;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Validators.AuthValidatorTests;
public class LoginRequestDtoValidatorTests
{
    private readonly LoginRequestDtoValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        // Arrange
        var model = new LoginRequestDto { Email = "", Password = "ValidPassword123" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("L'email est requis.");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        // Arrange
        var model = new LoginRequestDto { Email = "invalid-email", Password = "ValidPassword123" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("L'email n'est pas valide.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Email_Is_Valid()
    {
        // Arrange
        var model = new LoginRequestDto { Email = "test@example.com", Password = "ValidPassword123" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        // Arrange
        var model = new LoginRequestDto { Email = "test@example.com", Password = "" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Le mot de passe est requis.");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Too_Short()
    {
        // Arrange
        var model = new LoginRequestDto { Email = "test@example.com", Password = "short" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Le mot de passe doit contenir au moins 10 caractères.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Password_Is_Valid()
    {
        // Arrange
        var model = new LoginRequestDto { Email = "test@example.com", Password = "LongPassword123" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}
