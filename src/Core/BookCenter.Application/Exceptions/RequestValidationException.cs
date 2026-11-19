namespace BookCenter.Application.Exceptions
{
    /// <summary>
    /// Validation errors discovered inside a service (e.g. ASP.NET Identity password rules),
    /// as opposed to FluentValidation which runs before the action. Mapped to HTTP 400.
    /// Keys are camelCase field names, matching the request JSON.
    /// </summary>
    public class RequestValidationException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }

        public RequestValidationException(IDictionary<string, string[]> errors)
            : base("One or more validation errors occurred.")
        {
            Errors = errors;
        }
    }
}
