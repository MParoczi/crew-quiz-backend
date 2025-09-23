using System.Net;
using Backend.Enums;
using Backend.Extensions;
using Backend.Models.Configurations;
using Backend.Models.DTOs;
using Backend.Models.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;

namespace Backend.Handlers;

public class ExceptionHandler(IOptions<AppSettings> appSettings, ILogger<ExceptionHandler> logger) : IExceptionHandler
{
    private readonly AppSettings _appSettings = appSettings.Value;
    private readonly ILogger<ExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var handlerResult = exception switch
        {
            BusinessValidationException => HandleBusinessValidationException(httpContext, exception),
            _ => HandleFrameworkException(httpContext, exception)
        };

        await httpContext.Response.WriteAsJsonAsync(handlerResult, cancellationToken);
        return true;
    }

    private ExceptionDto HandleBusinessValidationException(HttpContext httpContext, Exception exception)
    {
        var correlationId = httpContext.GetOrCreateCorrelationId();

        // Extract user information if available
        var userId = httpContext.User?.FindFirst("UserId")?.Value;
        var username = httpContext.User?.Identity?.Name;

        // Log business validation exception with context
        using (httpContext.EnrichWithCorrelationId(correlationId))
        using (httpContext.EnrichWithUserContext(
                   userId != null ? int.Parse(userId) : null,
                   username))
        using (httpContext.EnrichWithRequestContext())
        {
            _logger.LogWarning(exception,
                "Business validation exception occurred. Exception: {ExceptionType} Message: {ExceptionMessage} TraceId: {TraceId}",
                exception.GetType().Name,
                exception.Message,
                httpContext.TraceIdentifier);
        }

        var response = new ExceptionDto
        {
            Level = LogLevel.Error.ToString(),
            Message = exception.Message,
            TraceId = httpContext.TraceIdentifier
        };

        httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        return response;
    }

    private ExceptionDto HandleFrameworkException(HttpContext httpContext, Exception exception)
    {
        var correlationId = httpContext.GetOrCreateCorrelationId();

        // Extract user information if available
        var userId = httpContext.User?.FindFirst("UserId")?.Value;
        var username = httpContext.User?.Identity?.Name;

        // Log framework exception with full context and stack trace
        using (httpContext.EnrichWithCorrelationId(correlationId))
        using (httpContext.EnrichWithUserContext(
                   userId != null ? int.Parse(userId) : null,
                   username))
        using (httpContext.EnrichWithRequestContext())
        {
            _logger.LogError(exception,
                "Framework exception occurred. Exception: {ExceptionType} Message: {ExceptionMessage} TraceId: {TraceId} StackTrace: {StackTrace}",
                exception.GetType().Name,
                exception.Message,
                httpContext.TraceIdentifier,
                exception.StackTrace);
        }

        var response = new ExceptionDto
        {
            Level = LogLevel.Critical.ToString(),
            Message = exception.Message,
            TraceId = httpContext.TraceIdentifier,
            Details = _appSettings.Environment == AppEnvironment.Development.GetDescription() ? exception.Data : null
        };

        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        return response;
    }
}