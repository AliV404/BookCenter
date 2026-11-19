using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Text.Json;

namespace BookCenter.API.Infrastructure
{
    /// <summary>
    /// Runs the matching FluentValidation validator for every action argument and returns
    /// a 400 ValidationProblemDetails when it fails. Replaces the deprecated
    /// FluentValidation.AspNetCore auto-validation package.
    /// Error keys are camelCase so they match the JSON field names the frontend sends.
    /// </summary>
    public sealed class ValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var services = context.HttpContext.RequestServices;

            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument is null) continue;

                var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
                if (services.GetService(validatorType) is not IValidator validator) continue;

                var result = await validator.ValidateAsync(new ValidationContext<object>(argument), context.HttpContext.RequestAborted);
                foreach (var error in result.Errors)
                    context.ModelState.AddModelError(JsonNamingPolicy.CamelCase.ConvertName(error.PropertyName), error.ErrorMessage);
            }

            if (!context.ModelState.IsValid)
            {
                var factory = services.GetRequiredService<ProblemDetailsFactory>();
                var problem = factory.CreateValidationProblemDetails(context.HttpContext, context.ModelState);
                context.Result = new BadRequestObjectResult(problem);
                return;
            }

            await next();
        }
    }
}
