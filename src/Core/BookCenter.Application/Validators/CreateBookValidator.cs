using BookCenter.Application.DTOs.Books;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.Validators
{
    public class CreateBookValidator:AbstractValidator<CreateBookDto>
    {
        public CreateBookValidator()
        {
            RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Book name is required.")
            .MaximumLength(200).WithMessage("Book name cannot exceed 200 characters.");

            RuleFor(x => x.ISBN)
            .NotEmpty().WithMessage("ISBN is required.")
            .Length(13).WithMessage("ISBN must be exactly 13 characters.");

            RuleFor(x => x.PublishYear)
            .InclusiveBetween(1500, DateTime.UtcNow.Year)
            .WithMessage($"Publish year must be between 1500 and {DateTime.UtcNow.Year}");

            RuleFor(x => x.CopiesAvailable)
            .GreaterThanOrEqualTo(0).WithMessage("Copies available must be 0 or greater.");

            RuleFor(x => x.AuthorId)
                .GreaterThan(0).WithMessage("Author is required.");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category is required.");

            RuleFor(x => x.PublisherId)
                .GreaterThan(0).WithMessage("Publisher is required.");

        }
    }
}
