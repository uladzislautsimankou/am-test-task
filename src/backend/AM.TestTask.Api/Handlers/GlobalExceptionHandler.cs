using AM.TestTask.Business.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AM.TestTask.Api.Handlers;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title, message) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation Error", exception.Message),

            DataRetrievalException => (StatusCodes.Status500InternalServerError, "Data Retrieval Error", "An error occurred while loading data. Please try again later."),

            SynchronizationException => (StatusCodes.Status500InternalServerError, "Synchronization Error", "External synchronization failed."),

            _ => (StatusCodes.Status500InternalServerError, "Server Error", "Internal Server Error")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = message,
            Instance = httpContext.Request.Path
        };

        if (exception is ValidationException validationEx)
        {
            // FluentValidation errors
            problemDetails.Extensions["errors"] = validationEx.Errors
                .Select(e => new { Field = e.PropertyName, Error = e.ErrorMessage });
        }

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
