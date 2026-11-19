using BookCenter.Application.DTOs.Authors;
using FluentValidation;

namespace BookCenter.Application.Validators
{
    public class CreateAuthorValidator : AbstractValidator<CreateAuthorDto>
    {
        public CreateAuthorValidator()
        {
            RuleFor(x => x.Name).ValidName(150);
            RuleFor(x => x.Biography).MaximumLength(2000);
        }
    }

    public class UpdateAuthorValidator : AbstractValidator<UpdateAuthorDto>
    {
        public UpdateAuthorValidator()
        {
            RuleFor(x => x.Name).ValidName(150);
            RuleFor(x => x.Biography).MaximumLength(2000);
        }
    }
}
