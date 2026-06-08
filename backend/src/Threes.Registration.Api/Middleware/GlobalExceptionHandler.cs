using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Threes.Registration.Application.Common.Exceptions;
using Threes.Registration.Domain.Common;
using ValidationException = Threes.Registration.Application.Common.Exceptions.ValidationException;

namespace Threes.Registration.Api.Middleware;

// one place that turns every known exception type into a consistent rfc7807
// problem-details response. registered with AddExceptionHandler so it catches
// anything that escapes a handler. the status codes line up with the task:
// 400 for validation, 404 for not found, 409 for duplicate email/mobile.
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problem = MapToProblem(exception, httpContext);

        if (problem.Status >= StatusCodes.Status500InternalServerError)
        {
            // only the unexpected stuff gets logged at error with the stack.
            _logger.LogError(exception, "unhandled exception for {Path}", httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync((object)problem, cancellationToken);
        return true;
    }

    private ProblemDetails MapToProblem(Exception exception, HttpContext httpContext)
    {
        switch (exception)
        {
            case ValidationException validation:
                return new ValidationProblemDetails(validation.Errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "One or more validation errors occurred.",
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Instance = httpContext.Request.Path,
                };

            case ConflictException conflict:
                var conflictProblem = BaseProblem(
                    StatusCodes.Status409Conflict,
                    "Conflict",
                    conflict.Message,
                    httpContext);
                if (conflict.Field is not null)
                {
                    conflictProblem.Extensions["field"] = conflict.Field;
                }

                return conflictProblem;

            case NotFoundException notFound:
                return BaseProblem(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    notFound.Message,
                    httpContext);

            case DomainException domain:
                // a domain rule slipped past the validators. treat as bad input.
                return BaseProblem(
                    StatusCodes.Status400BadRequest,
                    "Invalid request",
                    domain.Message,
                    httpContext);

            default:
                return BaseProblem(
                    StatusCodes.Status500InternalServerError,
                    "Server error",
                    _environment.IsDevelopment()
                        ? exception.Message
                        : "An unexpected error occurred.",
                    httpContext);
        }
    }

    private static ProblemDetails BaseProblem(int status, string title, string detail, HttpContext httpContext) =>
        new()
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path,
        };
}
