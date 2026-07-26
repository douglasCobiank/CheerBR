using System.Net;
using Cheer.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Cheer.Api.ExceptionHandling;

/// <summary>
/// Mapeia <see cref="NotFoundException"/> para HTTP 404 com ProblemDetails.
/// Outras excecoes continuam caindo no pipeline padrao do AddProblemDetails
/// (500 sem detalhes em producao).
/// </summary>
public class NotFoundExceptionHandler : IExceptionHandler
{
    private readonly ILogger<NotFoundExceptionHandler> _logger;

    public NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not NotFoundException notFound)
        {
            return false;
        }

        _logger.LogWarning(exception, "Entidade nao encontrada: {Message}", notFound.Message);

        var problem = new ProblemDetails
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.5",
            Title = "Not Found",
            Status = (int)HttpStatusCode.NotFound,
            Detail = notFound.Message,
            Instance = httpContext.Request.Path,
        };
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}
