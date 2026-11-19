using BookCenter.Application.DTOs.Readers;
using FluentValidation;

namespace BookCenter.Application.Validators
{
    internal static class ReaderRules
    {
        public const string PhonePattern = @"^\+?[0-9\s()\-]{7,20}$";
    }

    public class CreateReaderValidator : AbstractValidator<CreateReaderDto>
    {
        public CreateReaderValidator()
        {
            RuleFor(x => x.Name).ValidName(100);
            RuleFor(x => x.Surname).ValidName(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(x => x.Phone).NotEmpty().Matches(ReaderRules.PhonePattern).WithMessage("Phone number is not valid.");
        }
    }

    public class UpdateReaderValidator : AbstractValidator<UpdateReaderDto>
    {
        public UpdateReaderValidator()
        {
            RuleFor(x => x.Name).ValidName(100);
            RuleFor(x => x.Surname).ValidName(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(x => x.Phone).NotEmpty().Matches(ReaderRules.PhonePattern).WithMessage("Phone number is not valid.");
        }
    }
}
