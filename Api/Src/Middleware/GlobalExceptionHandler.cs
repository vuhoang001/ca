using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.Exceptions;

namespace Api.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception for request {Path}", httpContext.Request.Path);

        var (statusCode, title, errors) = exception switch
        {
            ValidationException validationException => (
                400,
                "Validation failed",
                validationException.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray())
            ),
            AppException appException => (
                appException.StatusCode,
                appException.Message,
                new Dictionary<string, string[]>()
            ),
            _ => (
                500,
                "An unexpected error occurred.",
                new Dictionary<string, string[]>()
            )
        };

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode == 500 ? null : exception.Message,
            Instance = httpContext.Request.Path
        };

        if (errors.Count > 0)
        {
            problem.Extensions["errors"] = errors;
        }

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}