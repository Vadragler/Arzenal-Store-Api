using Arzenal.Shared.Dtos.DTOs.RegisterDto;
using Arzenal.Shared.Dtos.Validators.AuthDtoValidators;
using FluentValidation.TestHelper;
using Xunit;

namespace TestApi.Validators.AuthValidatorTests
{
    public class RegisterRequestDtoValidatorTests
    {
        private readonly RegisterRequestDtoValidator _validator = new();

        [Fact]
        public void Should_Have_Error_When_Username_Is_Empty()
        {
            var model = new RegisterRequestDto { Username = "", Email = "test@mail.com", Password = "1234567890", Token = "token" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Username);
        }

        [Fact]
        public void Should_Have_Error_When_Username_Is_Too_Short()
        {
            var model = new RegisterRequestDto { Username = "ab", Email = "test@mail.com", Password = "1234567890", Token = "token" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Username);
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Empty()
        {
            var model = new RegisterRequestDto { Username = "validuser", Email = "", Password = "1234567890", Token = "token" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Invalid()
        {
            var model = new RegisterRequestDto { Username = "validuser", Email = "invalidemail", Password = "1234567890", Token = "token" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Is_Empty()
        {
            var model = new RegisterRequestDto { Username = "validuser", Email = "test@mail.com", Password = "", Token = "token" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Is_Too_Short()
        {
            var model = new RegisterRequestDto { Username = "validuser", Email = "test@mail.com", Password = "short", Token = "token" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Should_Have_Error_When_Token_Is_Empty()
        {
            var model = new RegisterRequestDto { Username = "validuser", Email = "test@mail.com", Password = "1234567890", Token = "" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Token);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Model_Is_Valid()
        {
            var model = new RegisterRequestDto
            {
                Username = "validuser",
                Email = "test@mail.com",
                Password = "1234567890",
                Token = "validtoken"
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
