using BookCenter.Application.Common;
using BookCenter.Application.DTOs.Auth;
using BookCenter.Application.DTOs.Users;
using FluentValidation;

namespace BookCenter.Application.Validators
{
    // Password strength (length, digit, letter) is enforced by ASP.NET Identity's password options.

    public class UserQueryValidator : AbstractValidator<UserQuery>
    {
        public UserQueryValidator()
        {
            RuleFor(x => x.Role)
                .Must(role => role is null || Roles.All.Contains(role, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"Role must be one of: {string.Join(", ", Roles.All)}.");
        }
    }

    public class ResetPasswordValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordValidator()
        {
            RuleFor(x => x.NewPassword).NotEmpty().MaximumLength(128);
        }
    }

    public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.CurrentPassword).NotEmpty();
            RuleFor(x => x.NewPassword).NotEmpty().MaximumLength(128)
                .NotEqual(x => x.CurrentPassword).WithMessage("The new password must be different from the current one.");
        }
    }
}
