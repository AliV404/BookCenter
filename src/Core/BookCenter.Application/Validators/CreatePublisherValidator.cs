using BookCenter.Application.DTOs.Publishers;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.Validators
{
    public class CreatePublisherValidator:AbstractValidator<CreatePublisherDto>
    {
        public CreatePublisherValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name field is required")
            .Matches(@"[A-za-z\s0-9]+$").WithMessage("Only letters, numbers and spaces are allowed.");
        }
    }
}
