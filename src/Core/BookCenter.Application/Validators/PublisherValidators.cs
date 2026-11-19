using BookCenter.Application.DTOs.Publishers;
using FluentValidation;

namespace BookCenter.Application.Validators
{
    public class CreatePublisherValidator : AbstractValidator<CreatePublisherDto>
    {
        public CreatePublisherValidator()
        {
            RuleFor(x => x.Name).ValidName(150);
        }
    }

    public class UpdatePublisherValidator : AbstractValidator<UpdatePublisherDto>
    {
        public UpdatePublisherValidator()
        {
            RuleFor(x => x.Name).ValidName(150);
        }
    }
}
