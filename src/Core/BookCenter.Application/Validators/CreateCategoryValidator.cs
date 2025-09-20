using BookCenter.Application.DTOs.Categories;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.Validators
{
    public class CreateCategoryValidator:AbstractValidator<CreateCategoyDto>
    {
        public CreateCategoryValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Name field is required!")
                .MaximumLength(100).WithMessage("Category Name must be less than 100.")
                .MinimumLength(2).WithMessage("Category Name characters must be between 2-100.")
                .Matches(@"[A-za-z\s0-9]+$").WithMessage("Only letters, numbers and spaces are allowed.")
                .Must(name => name == name.Trim()).WithMessage("Name cannot start or end with whitespace.");
        }
    }
}
