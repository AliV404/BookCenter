using BookCenter.Application.DTOs.Books;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.Validators
{
    public class UpdateBookValidator:AbstractValidator<UpdateBookDto>
    {
        public UpdateBookValidator()
        {
            //RuleFor(x => x.Id)
            //.GreaterThan(0).WithMessage("Book Id must be valid.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Book name is required.")
                .MaximumLength(200);

            RuleFor(x => x.ISBN)
                .NotEmpty().WithMessage("ISBN is required.")
                .Length(13);

            RuleFor(x => x.PublishYear)
                .InclusiveBetween(1500, DateTime.UtcNow.Year);

            RuleFor(x => x.CopiesAvailable)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.AuthorId)
                .GreaterThan(0);

            RuleFor(x => x.CategoryId)
                .GreaterThan(0);

            RuleFor(x => x.PublisherId)
                .GreaterThan(0);
        }
    }
}
