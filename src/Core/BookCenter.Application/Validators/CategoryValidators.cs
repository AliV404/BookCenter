using BookCenter.Application.DTOs.Categories;
using FluentValidation;

namespace BookCenter.Application.Validators
{
    public class CreateCategoryValidator : AbstractValidator<CreateCategoryDto>
    {
        public CreateCategoryValidator()
        {
            RuleFor(x => x.Name).ValidName(maxLength: 100, minLength: 2);
        }
    }

    public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryDto>
    {
        public UpdateCategoryValidator()
        {
            RuleFor(x => x.Name).ValidName(maxLength: 100, minLength: 2);
        }
    }
}
