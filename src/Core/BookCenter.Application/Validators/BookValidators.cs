using BookCenter.Application.DTOs.Books;
using FluentValidation;

namespace BookCenter.Application.Validators
{
    public class CreateBookValidator : AbstractValidator<CreateBookDto>
    {
        public CreateBookValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Book name is required.").MaximumLength(200);
            RuleFor(x => x.ISBN).ValidIsbn();
            RuleFor(x => x.PublishYear).ValidPublishYear();
            RuleFor(x => x.TotalCopies).InclusiveBetween(0, 10_000);
            RuleFor(x => x.AuthorId).ValidId("Author");
            RuleFor(x => x.CategoryId).ValidId("Category");
            RuleFor(x => x.PublisherId).ValidId("Publisher");
        }
    }

    public class UpdateBookValidator : AbstractValidator<UpdateBookDto>
    {
        public UpdateBookValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Book name is required.").MaximumLength(200);
            RuleFor(x => x.ISBN).ValidIsbn();
            RuleFor(x => x.PublishYear).ValidPublishYear();
            RuleFor(x => x.TotalCopies).InclusiveBetween(0, 10_000);
            RuleFor(x => x.AuthorId).ValidId("Author");
            RuleFor(x => x.CategoryId).ValidId("Category");
            RuleFor(x => x.PublisherId).ValidId("Publisher");
        }
    }
}
