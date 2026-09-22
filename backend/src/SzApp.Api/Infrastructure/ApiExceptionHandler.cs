using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SzApp.Domain;

namespace SzApp.Api.Infrastructure;

public sealed class ApiExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, code) = exception switch
        {
            AntiforgeryValidationException =>
                (StatusCodes.Status400BadRequest, "CSRF token nije ispravan", "security.invalid-antiforgery-token"),
            DomainRuleException domainException =>
                (StatusCodes.Status422UnprocessableEntity, "Poslovno pravilo nije zadovoljeno", domainException.Code),
            DbUpdateConcurrencyException =>
                (StatusCodes.Status409Conflict, "Podatak je u međuvremenu izmenjen", "concurrency.conflict"),
            KeyNotFoundException =>
                (StatusCodes.Status404NotFound, "Podatak nije pronađen", "resource.not-found"),
            BadHttpRequestException =>
                (StatusCodes.Status400BadRequest, "Zahtev nije ispravan", "request.invalid"),
            UnauthorizedAccessException =>
                (StatusCodes.Status403Forbidden, "Pristup nije dozvoljen", "security.forbidden"),
            _ =>
                (StatusCodes.Status500InternalServerError, "Neočekivana greška", "server.error")
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled request failure. CorrelationId: {CorrelationId}", httpContext.TraceIdentifier);
        }

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status == StatusCodes.Status500InternalServerError ? null : exception.Message,
                Type = $"https://httpstatuses.com/{status}",
                Extensions = { ["code"] = code }
            }
        });
    }
}
