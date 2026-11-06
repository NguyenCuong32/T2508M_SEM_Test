using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SunriseHotel.Application.Common.Exceptions;
using SunriseHotel.Domain.Exceptions;

namespace SunriseHotel.Api.ErrorHandling;

/// <summary>Maps application exceptions to RFC 7807 ProblemDetails responses with the right HTTP status.</summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, title, detail) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed", exception.Message),
            DomainException => (StatusCodes.Status400BadRequest, "Business rule violated", exception.Message),
            NotFoundException => (StatusCodes.Status404NotFound, "Not found", exception.Message),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict", exception.Message),
            AiQuotaExceededException => (StatusCodes.Status429TooManyRequests, "AI quota exceeded", exception.Message),
            AiInvalidResponseException => (StatusCodes.Status502BadGateway, "Invalid AI response", exception.Message),
            AiConfigurationException => (StatusCodes.Status500InternalServerError, "AI not configured", exception.Message),
            AiServiceException => (StatusCodes.Status503ServiceUnavailable, "AI service unavailable", exception.Message),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Bad request", exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "Server error", "An unexpected error occurred.")
        };

        if (status >= 500) logger.LogError(exception, "Request failed: {Message}", exception.Message);
        else logger.LogWarning("Request rejected ({Status}): {Message}", status, exception.Message);

        var problem = new ProblemDetails { Status = status, Title = title, Detail = detail };

        switch (exception)
        {
            case ValidationException validation:
                problem.Extensions["errors"] = validation.Errors;
                if (validation.Details is not null) problem.Extensions["details"] = validation.Details;
                break;
            case AiQuotaExceededException { RetryAfter: { } retryAfter }:
                context.Response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();
                problem.Extensions["retryAfterSeconds"] = (int)Math.Ceiling(retryAfter.TotalSeconds);
                break;
            case AiInvalidResponseException { RawResponse: { } raw }:
                problem.Extensions["rawResponse"] = raw.Length > 2000 ? raw[..2000] : raw;
                break;
        }

        context.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problem,
            Exception = exception
        });
    }
}
