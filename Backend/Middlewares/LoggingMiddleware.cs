using System.Diagnostics;
using Backend.Extensions;

namespace Backend.Middlewares;

public class LoggingMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for health check and static content endpoints
        if (ShouldSkipLogging(context))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var correlationId = context.GetOrCreateCorrelationId();

        // Enrich log context with correlation ID and request information
        using var correlationScope = context.EnrichWithCorrelationId(correlationId);
        using var requestScope = context.EnrichWithRequestContext();

        try
        {
            // Log incoming request
            LogRequestStart(context, correlationId);

            // Process request
            await _next(context);

            stopwatch.Stop();

            // Log successful response
            LogRequestComplete(context, correlationId, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Log failed request
            LogRequestFailed(context, correlationId, stopwatch.ElapsedMilliseconds, ex);

            // Re-throw to let exception handling middleware process it
            throw;
        }
    }

    private static bool ShouldSkipLogging(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant();

        return path != null && (
            path.Contains("/health") ||
            path.Contains("/metrics") ||
            path.Contains("/favicon") ||
            path.Contains("/_framework") ||
            path.Contains("/swagger") ||
            path.StartsWith("/css") ||
            path.StartsWith("/js") ||
            path.StartsWith("/images")
        );
    }

    private void LogRequestStart(HttpContext context, string correlationId)
    {
        var request = context.Request;

        _logger.LogInformation(
            "Request started: {Method} {Path}{QueryString} from {RemoteIp} | CorrelationId: {CorrelationId}",
            request.Method,
            request.Path.Value,
            request.QueryString.Value,
            context.Connection.RemoteIpAddress?.ToString(),
            correlationId
        );

        // Log request headers in debug mode
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var headers = request.Headers
                .Where(h => !IsSensitiveHeader(h.Key))
                .ToDictionary(h => h.Key, h => string.Join(", ", h.Value));

            _logger.LogDebug("Request headers: {@Headers}", headers);
        }
    }

    private void LogRequestComplete(HttpContext context, string correlationId, long elapsedMs)
    {
        var request = context.Request;
        var response = context.Response;

        var logLevel = DetermineLogLevel(response.StatusCode, elapsedMs);

        _logger.Log(logLevel,
            "Request completed: {Method} {Path} responded {StatusCode} in {ElapsedMs}ms | CorrelationId: {CorrelationId}",
            request.Method,
            request.Path.Value,
            response.StatusCode,
            elapsedMs,
            correlationId
        );

        // Log slow requests as warnings
        if (elapsedMs > 5000) // 5 seconds threshold
            _logger.LogWarning(
                "Slow request detected: {Method} {Path} took {ElapsedMs}ms | CorrelationId: {CorrelationId}",
                request.Method,
                request.Path.Value,
                elapsedMs,
                correlationId
            );
    }

    private void LogRequestFailed(HttpContext context, string correlationId, long elapsedMs, Exception exception)
    {
        var request = context.Request;

        _logger.LogError(exception,
            "Request failed: {Method} {Path} failed after {ElapsedMs}ms | CorrelationId: {CorrelationId} | Error: {ErrorMessage}",
            request.Method,
            request.Path.Value,
            elapsedMs,
            correlationId,
            exception.Message
        );
    }

    private static LogLevel DetermineLogLevel(int statusCode, long elapsedMs)
    {
        // Error responses
        if (statusCode >= 500)
            return LogLevel.Error;

        // Client errors  
        if (statusCode >= 400)
            return LogLevel.Warning;

        // Slow responses
        if (elapsedMs > 3000) // 3 seconds threshold
            return LogLevel.Warning;

        // Successful responses
        return LogLevel.Information;
    }

    private static bool IsSensitiveHeader(string headerName)
    {
        var sensitiveHeaders = new[]
        {
            "authorization",
            "cookie",
            "x-api-key",
            "x-auth-token",
            "authentication",
            "proxy-authorization"
        };

        return sensitiveHeaders.Contains(headerName.ToLowerInvariant());
    }

    private static IDisposable? EnrichWithUserInformation(HttpContext context)
    {
        if (!context.User?.Identity?.IsAuthenticated == true)
            return null;

        var userId = context.User.FindFirst("userId")?.Value;
        var username = context.User.FindFirst("username")?.Value ?? context.User.Identity.Name;

        if (int.TryParse(userId, out var userIdInt)) return context.EnrichWithUserContext(userIdInt, username);

        if (!string.IsNullOrEmpty(username)) return context.EnrichWithUserContext(username: username);

        return null;
    }
}

public static class LoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<LoggingMiddleware>();
    }
}