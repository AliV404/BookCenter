using BookCenter.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BookCenter.API.Infrastructure
{
    /// <summary>
    /// Turns exceptions thrown by services into RFC 7807 ProblemDetails responses:
    /// RequestValidationException → 400, AuthenticationFailedException → 401,
    /// NotFoundException → 404, ConflictException → 409, anything else → 500 (details hidden, logged).
    /// </summary>
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetailsService;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
        {
            _problemDetailsService = problemDetailsService;
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            ProblemDetails problem = exception switch
            {
                RequestValidationException validation => new ValidationProblemDetails(validation.Errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "One or more validation errors occurred."
                },
                AuthenticationFailedException => Problem(StatusCodes.Status401Unauthorized, "Unauthorized", exception.Message),
                NotFoundException => Problem(StatusCodes.Status404NotFound, "Not found", exception.Message),
                ConflictException => Problem(StatusCodes.Status409Conflict, "Conflict", exception.Message),
                _ => Problem(StatusCodes.Status500InternalServerError, "Server error", "An unexpected error occurred.")
            };

            if (problem.Status == StatusCodes.Status500InternalServerError)
                _logger.LogError(exception, "Unhandled exception for {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

            httpContext.Response.StatusCode = problem.Status!.Value;
            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problem
            });
        }

        private static ProblemDetails Problem(int status, string title, string detail) =>
            new() { Status = status, Title = title, Detail = detail };
    }
}
