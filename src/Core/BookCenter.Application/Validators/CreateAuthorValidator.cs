using BookCenter.Application.DTOs.Authors;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.Validators
{
    public class CreateAuthorValidator:AbstractValidator<CreateAuthorDto>
    {
        public CreateAuthorValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name field is required")
            .Matches(@"[A-za-z\s0-9]+$").WithMessage("Only letters, numbers and spaces are allowed.");

            RuleFor(x => x.Biography).NotEmpty().WithMessage("Biography field is required")
            .Matches(@"[A-za-z\s0-9]+$").WithMessage("Only letters, numbers and spaces are allowed.");
        }
    }
}
