using BookCenter.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace BookCenter.API.Infrastructure
{
    /// <summary>
    /// Turns exceptions thrown by services into RFC 7807 ProblemDetails responses:
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
            var (status, title, detail) = exception switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "Not found", exception.Message),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict", exception.Message),
                _ => (StatusCodes.Status500InternalServerError, "Server error", "An unexpected error occurred.")
            };

            if (status == StatusCodes.Status500InternalServerError)
                _logger.LogError(exception, "Unhandled exception for {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

            httpContext.Response.StatusCode = status;
            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = { Status = status, Title = title, Detail = detail }
            });
        }
    }
}
