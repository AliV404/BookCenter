using BookCenter.Application.DTOs.Auth;
using FluentValidation;

namespace BookCenter.Application.Validators
{
    public class RegisterValidator : AbstractValidator<RegisterDto>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.Name).ValidName(100);
            RuleFor(x => x.Surname).ValidName(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(x => x.Phone).NotEmpty().Matches(ReaderRules.PhonePattern).WithMessage("Phone number is not valid.");
            // Strength rules (length, digit, letter) are enforced by ASP.NET Identity's password options.
            RuleFor(x => x.Password).NotEmpty().MaximumLength(128);
        }
    }

    public class LoginValidator : AbstractValidator<LoginDto>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}
