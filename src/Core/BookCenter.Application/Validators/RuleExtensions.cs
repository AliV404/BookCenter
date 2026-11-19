using FluentValidation;

namespace BookCenter.Application.Validators
{
    /// <summary>Reusable rules so Create and Update validators can't drift apart.</summary>
    public static class RuleExtensions
    {
        // \p{L} = any Unicode letter, so names like "Nizami Gəncəvi" or "Müller" are accepted.
        private const string NamePattern = @"^[\p{L}\p{N}\s.,'&()\-]+$";

        public static IRuleBuilderOptions<T, string> ValidName<T>(this IRuleBuilderInitial<T, string> rule, int maxLength, int minLength = 1)
        {
            return rule
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .Length(minLength, maxLength).WithMessage("{PropertyName} must be between {MinLength} and {MaxLength} characters.")
                .Must(v => v == v.Trim()).WithMessage("{PropertyName} cannot start or end with whitespace.")
                .Matches(NamePattern).WithMessage("{PropertyName} may contain only letters, numbers, spaces and . , ' & ( ) -");
        }

        public static IRuleBuilderOptions<T, string> ValidIsbn<T>(this IRuleBuilderInitial<T, string> rule)
        {
            return rule
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("ISBN is required.")
                .Matches(@"^\d{13}$").WithMessage("ISBN must be exactly 13 digits.");
        }

        public static IRuleBuilderOptions<T, int> ValidPublishYear<T>(this IRuleBuilder<T, int> rule)
        {
            return rule
                .InclusiveBetween(1450, DateTime.UtcNow.Year)
                .WithMessage("Publish year must be between {From} and {To}.");
        }

        public static IRuleBuilderOptions<T, int> ValidId<T>(this IRuleBuilder<T, int> rule, string entityName)
        {
            return rule.GreaterThan(0).WithMessage($"{entityName} is required.");
        }
    }
}
