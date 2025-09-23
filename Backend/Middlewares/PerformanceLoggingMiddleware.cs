using System.Diagnostics;
using Backend.Extensions;

namespace Backend.Middlewares;

public class PerformanceLoggingMiddleware
{
    private readonly ILogger<PerformanceLoggingMiddleware> _logger;
    private readonly RequestDelegate _next;
    private readonly PerformanceLoggingOptions _options;

    public PerformanceLoggingMiddleware(
        RequestDelegate next,
        ILogger<PerformanceLoggingMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _options = configuration.GetSection("PerformanceLogging").Get<PerformanceLoggingOptions>()
                   ?? new PerformanceLoggingOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.GetOrCreateCorrelationId();
        var stopwatch = Stopwatch.StartNew();
        var memoryBefore = GC.GetTotalMemory(false);

        using var correlationScope = context.EnrichWithCorrelationId(correlationId);
        using var requestScope = context.EnrichWithRequestContext();

        var requestPath = context.Request.Path.Value;
        var requestMethod = context.Request.Method;

        // Log request start for critical operations
        if (IsCriticalOperation(requestPath))
            _logger.LogInformation("Starting critical operation {RequestMethod} {RequestPath} with CorrelationId {CorrelationId}",
                requestMethod, requestPath, correlationId);

        Exception? exception = null;
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            var memoryAfter = GC.GetTotalMemory(false);
            var memoryUsed = memoryAfter - memoryBefore;

            LogPerformanceMetrics(context, stopwatch.ElapsedMilliseconds, memoryUsed, exception);
        }
    }

    private void LogPerformanceMetrics(HttpContext context, long elapsedMs, long memoryUsed, Exception? exception)
    {
        var requestPath = context.Request.Path.Value;
        var requestMethod = context.Request.Method;
        var statusCode = context.Response.StatusCode;
        var correlationId = context.GetOrCreateCorrelationId();

        var performanceData = new
        {
            RequestMethod = requestMethod,
            RequestPath = requestPath,
            StatusCode = statusCode,
            ElapsedMs = elapsedMs,
            MemoryUsedBytes = memoryUsed,
            CorrelationId = correlationId,
            IsCriticalOperation = IsCriticalOperation(requestPath),
            ThreadId = Environment.CurrentManagedThreadId,
            Environment.ProcessorCount,
            WorkingSetBytes = Environment.WorkingSet,
            HasException = exception != null
        };

        // Determine log level based on performance thresholds
        var logLevel = DetermineLogLevel(elapsedMs, statusCode, exception != null);

        switch (logLevel)
        {
            case LogLevel.Critical:
                _logger.LogCritical("Critical performance issue detected for {RequestMethod} {RequestPath}. " +
                                    "Elapsed: {ElapsedMs}ms, Memory: {MemoryUsedBytes} bytes, Status: {StatusCode} {@PerformanceData}",
                    requestMethod, requestPath, elapsedMs, memoryUsed, statusCode, performanceData);
                break;

            case LogLevel.Error:
                _logger.LogError("Poor performance detected for {RequestMethod} {RequestPath}. " +
                                 "Elapsed: {ElapsedMs}ms, Memory: {MemoryUsedBytes} bytes, Status: {StatusCode} {@PerformanceData}",
                    requestMethod, requestPath, elapsedMs, memoryUsed, statusCode, performanceData);
                break;

            case LogLevel.Warning:
                _logger.LogWarning("Slow operation detected for {RequestMethod} {RequestPath}. " +
                                   "Elapsed: {ElapsedMs}ms, Memory: {MemoryUsedBytes} bytes, Status: {StatusCode} {@PerformanceData}",
                    requestMethod, requestPath, elapsedMs, memoryUsed, statusCode, performanceData);
                break;

            case LogLevel.Information:
                if (IsCriticalOperation(requestPath) || _options.LogAllRequests)
                    _logger.LogInformation("Request completed {RequestMethod} {RequestPath}. " +
                                           "Elapsed: {ElapsedMs}ms, Memory: {MemoryUsedBytes} bytes, Status: {StatusCode} {@PerformanceData}",
                        requestMethod, requestPath, elapsedMs, memoryUsed, statusCode, performanceData);
                break;

            default:
                _logger.LogDebug("Request completed {RequestMethod} {RequestPath}. " +
                                 "Elapsed: {ElapsedMs}ms, Memory: {MemoryUsedBytes} bytes, Status: {StatusCode} {@PerformanceData}",
                    requestMethod, requestPath, elapsedMs, memoryUsed, statusCode, performanceData);
                break;
        }

        // Log additional metrics for critical operations
        if (IsCriticalOperation(requestPath)) LogCriticalOperationMetrics(requestMethod, requestPath, elapsedMs, memoryUsed, performanceData);
    }

    private LogLevel DetermineLogLevel(long elapsedMs, int statusCode, bool hasException)
    {
        if (hasException || statusCode >= 500)
            return LogLevel.Critical;

        if (elapsedMs > _options.CriticalThresholdMs)
            return LogLevel.Error;

        if (elapsedMs > _options.SlowThresholdMs || statusCode >= 400)
            return LogLevel.Warning;

        if (elapsedMs > _options.InfoThresholdMs)
            return LogLevel.Information;

        return LogLevel.Debug;
    }

    private bool IsCriticalOperation(string? requestPath)
    {
        if (string.IsNullOrEmpty(requestPath))
            return false;

        var criticalPaths = _options.CriticalOperationPaths;

        return criticalPaths.Any(path =>
            requestPath.StartsWith(path, StringComparison.OrdinalIgnoreCase));
    }

    private void LogCriticalOperationMetrics(string requestMethod, string? requestPath,
        long elapsedMs, long memoryUsed, object performanceData)
    {
        var criticalMetrics = new
        {
            RequestMethod = requestMethod,
            RequestPath = requestPath,
            ElapsedMs = elapsedMs,
            MemoryUsedBytes = memoryUsed,
            GCGen0Collections = GC.CollectionCount(0),
            GCGen1Collections = GC.CollectionCount(1),
            GCGen2Collections = GC.CollectionCount(2),
            TotalMemoryBytes = GC.GetTotalMemory(false),
            ThreadPoolWorkerThreads = ThreadPool.ThreadCount,
            ProcessorUsage = GetProcessorUsagePercentage(),
            Timestamp = DateTimeOffset.UtcNow
        };

        _logger.LogInformation("Critical operation metrics for {RequestMethod} {RequestPath}: {@CriticalMetrics}",
            requestMethod, requestPath, criticalMetrics);
    }

    private double GetProcessorUsagePercentage()
    {
        try
        {
            // This is a simplified CPU usage calculation
            // In production, you might want to use more sophisticated monitoring
            return Environment.ProcessorCount > 0 ? (double)Environment.WorkingSet / (1024 * 1024 * Environment.ProcessorCount) : 0.0;
        }
        catch
        {
            return 0.0;
        }
    }
}

public class PerformanceLoggingOptions
{
    public int SlowThresholdMs { get; set; } = 1000;
    public int CriticalThresholdMs { get; set; } = 5000;
    public int InfoThresholdMs { get; set; } = 500;
    public bool LogAllRequests { get; set; }

    public List<string> CriticalOperationPaths { get; set; } = new()
    {
        "/api/authentication",
        "/api/gameflow",
        "/api/currentgame",
        "/api/quiz",
        "/GameHub"
    };
}