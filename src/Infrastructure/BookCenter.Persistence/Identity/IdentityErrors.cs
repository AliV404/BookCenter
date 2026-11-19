using BookCenter.Application.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace BookCenter.Persistence.Identity
{
    internal static class IdentityErrors
    {
        /// <summary>Maps Identity's error codes onto the request field they belong to (a 400 with field errors).</summary>
        public static RequestValidationException ToValidationException(IEnumerable<IdentityError> errors, string passwordField = "password")
        {
            var byField = errors
                .GroupBy(e => e.Code switch
                {
                    "PasswordMismatch" => "currentPassword",
                    var c when c.StartsWith("Password") => passwordField,
                    var c when c.Contains("Email") || c.Contains("UserName") => "email",
                    _ => ""
                })
                .ToDictionary(g => g.Key, g => g.Select(e => e.Code == "PasswordMismatch" ? "Current password is incorrect." : e.Description).ToArray());

            return new RequestValidationException(byField);
        }

        /// <summary>For results that can only fail because of a bug or a concurrent change.</summary>
        public static void ThrowIfFailed(this IdentityResult result, string action)
        {
            if (!result.Succeeded)
                throw new InvalidOperationException($"Could not {action}: " + string.Join(" ", result.Errors.Select(e => e.Description)));
        }
    }
}
