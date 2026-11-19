using BookCenter.Application.DTOs.BorrowRecords;
using FluentValidation;

namespace BookCenter.Application.Validators
{
    public class CreateBorrowRecordValidator : AbstractValidator<CreateBorrowRecordDto>
    {
        public const int MaxLoanDays = 90;

        public CreateBorrowRecordValidator()
        {
            RuleFor(x => x.BookId).ValidId("Book");
            RuleFor(x => x.ReaderId).ValidId("Reader");
            RuleFor(x => x.DueDate)
                .GreaterThan(_ => DateTime.UtcNow).WithMessage("Due date must be in the future.")
                .LessThanOrEqualTo(_ => DateTime.UtcNow.AddDays(MaxLoanDays)).WithMessage($"Loan period cannot exceed {MaxLoanDays} days.");
        }
    }
}
