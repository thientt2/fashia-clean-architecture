using Fashia.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Fashia.Web.Infrastructure;

public class ProblemDetailsExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;

    public ProblemDetailsExceptionHandler(IProblemDetailsService problemDetailsService) =>
        _problemDetailsService = problemDetailsService;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var (statusCode, problemDetails) = exception switch
        {
            ValidationException ve => (
                StatusCodes.Status400BadRequest,
                (ProblemDetails)
                    new ValidationProblemDetails(ve.Errors)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                        Title = "One or more validation errors occurred.",
                    }
            ),
            BadHttpRequestException bhe => (
                StatusCodes.Status400BadRequest,
                CreateProblemDetails(
                    StatusCodes.Status400BadRequest,
                    "Bad request",
                    bhe.Message,
                    "https://tools.ietf.org/html/rfc9110#section-15.5.1"
                )
            ),
            NotFoundException ne => (
                StatusCodes.Status404NotFound,
                new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    Title = "The specified resource was not found.",
                    Detail = ne.Message,
                }
            ),
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Unauthorized",
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                }
            ),
            ForbiddenAccessException => (
                StatusCodes.Status403Forbidden,
                new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Forbidden",
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                }
            ),
            ConflictException ce => (
                StatusCodes.Status409Conflict,
                CreateProblemDetails(
                    StatusCodes.Status409Conflict,
                    "Conflict",
                    ce.Message,
                    "https://tools.ietf.org/html/rfc9110#section-15.5.10"
                )
            ),
            IdempotencyKeyConflictException or DuplicateRequestInProgressException => (
                StatusCodes.Status409Conflict,
                CreateProblemDetails(
                    StatusCodes.Status409Conflict,
                    "Checkout request conflict",
                    exception.Message,
                    "https://tools.ietf.org/html/rfc9110#section-15.5.10"
                )
            ),
            CheckoutConflictException => (
                StatusCodes.Status409Conflict,
                CreateProblemDetails(
                    StatusCodes.Status409Conflict,
                    "Checkout conflict",
                    exception.Message,
                    "https://tools.ietf.org/html/rfc9110#section-15.5.10"
                )
            ),
            InvalidVoucherException => (
                StatusCodes.Status422UnprocessableEntity,
                CreateProblemDetails(
                    StatusCodes.Status422UnprocessableEntity,
                    "Invalid voucher",
                    exception.Message,
                    "https://tools.ietf.org/html/rfc9110#section-15.5.21"
                )
            ),
            _ => (-1, null),
        };

        if (problemDetails is null)
            return false;

        httpContext.Response.StatusCode = statusCode;

        return await _problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = problemDetails }
        );
    }

    private static ProblemDetails CreateProblemDetails(
        int status,
        string title,
        string detail,
        string type
    ) =>
        new()
        {
            Status = status,
            Title = title,
            Detail = detail,
            Type = type,
        };
}
