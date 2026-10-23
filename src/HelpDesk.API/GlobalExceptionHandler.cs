using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Domain.Exceptions;

namespace HelpDesk.API;

// ReSharper disable once ClassNeverInstantiated.Global
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, 
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Incorrect input data"),
            DomainException => (StatusCodes.Status400BadRequest, "Invalid domain operation"),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Invalid operation"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            _ => (StatusCodes.Status500InternalServerError, "Internal server error")
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception while processing {Path}",
                httpContext.Request.Path);
        }
        
        bool isSafeClientError = statusCode < StatusCodes.Status500InternalServerError;
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = isSafeClientError
                ? exception.Message
                : "An unexpected error occurred.",
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}