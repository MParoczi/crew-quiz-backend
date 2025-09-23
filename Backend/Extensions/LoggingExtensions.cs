using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using SerilogLogger = Serilog.ILogger;
using MSLogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Extensions;

public static class LoggingExtensions
{
    public static IServiceCollection ConfigureSerilog(this IServiceCollection services, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        services.AddSingleton(Log.Logger);
        return services;
    }

    public static IDisposable EnrichWithCorrelationId(this HttpContext context, string correlationId)
    {
        return LogContext.PushProperty("CorrelationId", correlationId);
    }

    public static IDisposable EnrichWithUserContext(this HttpContext context, int? userId = null, string? username = null)
    {
        var properties = new List<IDisposable>();

        if (userId.HasValue) properties.Add(LogContext.PushProperty("UserId", userId.Value));

        if (!string.IsNullOrEmpty(username)) properties.Add(LogContext.PushProperty("Username", username));

        return new CompositeDisposable(properties);
    }

    public static IDisposable EnrichWithRequestContext(this HttpContext context)
    {
        var properties = new List<IDisposable>
        {
            LogContext.PushProperty("RequestMethod", context.Request.Method),
            LogContext.PushProperty("RequestPath", context.Request.Path.Value),
            LogContext.PushProperty("RequestQueryString", context.Request.QueryString.Value),
            LogContext.PushProperty("RemoteIpAddress", context.Connection.RemoteIpAddress?.ToString()),
            LogContext.PushProperty("UserAgent", context.Request.Headers.UserAgent.ToString())
        };

        return new CompositeDisposable(properties);
    }

    public static DbContextOptionsBuilder ConfigureSerilogLogging(this DbContextOptionsBuilder optionsBuilder, SerilogLogger logger)
    {
        return optionsBuilder
            .LogTo(message => logger.Information("[EF Core] {Message}", message))
            .EnableSensitiveDataLogging(false)
            .EnableDetailedErrors();
    }

    public static string GetOrCreateCorrelationId(this HttpContext? context)
    {
        const string correlationIdKey = "X-Correlation-ID";

        // Handle null context (e.g., in test environments)
        if (context == null) return Guid.NewGuid().ToString();

        if (context.Request.Headers.TryGetValue(correlationIdKey, out var correlationId)) return correlationId.ToString();

        var newCorrelationId = Guid.NewGuid().ToString();
        context.Response.Headers.Append(correlationIdKey, newCorrelationId);
        return newCorrelationId;
    }

    public static void LogMethodEntry(this MSLogger logger, string methodName, object? parameters = null)
    {
        if (parameters != null)
            logger.LogDebug("Entering method {MethodName} with parameters {@Parameters}", methodName, parameters);
        else
            logger.LogDebug("Entering method {MethodName}", methodName);
    }

    public static void LogMethodExit(this MSLogger logger, string methodName, object? result = null, long? elapsedMs = null)
    {
        if (result != null && elapsedMs.HasValue)
            logger.LogDebug("Exiting method {MethodName} with result {@Result} in {ElapsedMs}ms", methodName, result, elapsedMs);
        else if (elapsedMs.HasValue)
            logger.LogDebug("Exiting method {MethodName} in {ElapsedMs}ms", methodName, elapsedMs);
        else if (result != null)
            logger.LogDebug("Exiting method {MethodName} with result {@Result}", methodName, result);
        else
            logger.LogDebug("Exiting method {MethodName}", methodName);
    }

    public static void LogValidationErrors(this MSLogger logger, IEnumerable<string> validationErrors, object? context = null)
    {
        logger.LogWarning("Business validation failed with errors: {@ValidationErrors} Context: {@Context}",
            validationErrors, context);
    }

    public static void LogPerformanceMetrics(this MSLogger logger, string operationName, long elapsedMs,
        int threshold = 1000, object? context = null)
    {
        if (elapsedMs > threshold)
            logger.LogWarning("Slow operation detected: {OperationName} took {ElapsedMs}ms (threshold: {Threshold}ms) Context: {@Context}",
                operationName, elapsedMs, threshold, context);
        else
            logger.LogDebug("Operation {OperationName} completed in {ElapsedMs}ms Context: {@Context}",
                operationName, elapsedMs, context);
    }

    public static IDisposable LogOperationScope(this MSLogger logger, string operationName, object? parameters = null)
    {
        var correlationId = Guid.NewGuid().ToString();
        var stopwatch = Stopwatch.StartNew();

        logger.LogMethodEntry(operationName, parameters);

        return new OperationScope(logger, operationName, stopwatch, correlationId);
    }

    public static void LogMemoryUsage(this MSLogger logger, string context = "General", bool forceGC = false)
    {
        try
        {
            var memoryBefore = GC.GetTotalMemory(false);

            if (forceGC)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }

            var memoryAfter = GC.GetTotalMemory(false);
            var memoryUsageMB = memoryAfter / 1024 / 1024;

            var memoryMetrics = new
            {
                Context = context,
                TotalMemoryMB = memoryUsageMB,
                Gen0Collections = GC.CollectionCount(0),
                Gen1Collections = GC.CollectionCount(1),
                Gen2Collections = GC.CollectionCount(2),
                MemoryFreedMB = forceGC ? (memoryBefore - memoryAfter) / 1024 / 1024 : 0,
                Timestamp = DateTimeOffset.UtcNow
            };

            if (memoryUsageMB > 512) // Warning if over 512MB
                logger.LogWarning("High memory usage detected in {Context}: {@MemoryMetrics}",
                    context, memoryMetrics);
            else
                logger.LogInformation("Memory usage recorded for {Context}: {@MemoryMetrics}",
                    context, memoryMetrics);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to log memory usage for context {Context}", context);
        }
    }

    public static void LogSystemHealth(this MSLogger logger)
    {
        try
        {
            var process = Process.GetCurrentProcess();
            var uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime();

            var systemHealth = new
            {
                ProcessId = process.Id,
                process.ProcessName,
                WorkingSetMB = process.WorkingSet64 / 1024 / 1024,
                PrivateMemoryMB = process.PrivateMemorySize64 / 1024 / 1024,
                VirtualMemoryMB = process.VirtualMemorySize64 / 1024 / 1024,
                ThreadCount = process.Threads.Count,
                process.HandleCount,
                UptimeHours = Math.Round(uptime.TotalHours, 2),
                Environment.ProcessorCount,
                Environment.MachineName,
                OSVersion = Environment.OSVersion.ToString(),
                Is64BitOS = Environment.Is64BitOperatingSystem,
                Environment.Is64BitProcess,
                ThreadPoolWorkerThreads = ThreadPool.ThreadCount,
                ThreadPoolCompletedWorkItems = ThreadPool.CompletedWorkItemCount,
                Timestamp = DateTimeOffset.UtcNow
            };

            logger.LogInformation("System health snapshot: {@SystemHealth}", systemHealth);

            // Log warnings for concerning metrics
            if (systemHealth.WorkingSetMB > 1024) logger.LogWarning("High memory usage: Working set is {WorkingSetMB}MB", systemHealth.WorkingSetMB);

            if (systemHealth.ThreadCount > 100) logger.LogWarning("High thread count: {ThreadCount} threads active", systemHealth.ThreadCount);

            if (systemHealth.HandleCount > 1000) logger.LogWarning("High handle count: {HandleCount} handles in use", systemHealth.HandleCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to log system health snapshot");
        }
    }

    public static void LogDatabaseOperation(this MSLogger logger, string operation, string? tableName = null,
        long? elapsedMs = null, int? recordCount = null, bool success = true)
    {
        try
        {
            var dbOperation = new
            {
                Operation = operation,
                TableName = tableName ?? "Unknown",
                ElapsedMs = elapsedMs,
                RecordCount = recordCount,
                Success = success,
                Timestamp = DateTimeOffset.UtcNow
            };

            if (!success)
                logger.LogError("Database operation failed: {@DatabaseOperation}", dbOperation);
            else if (elapsedMs > 1000) // Slow query threshold
                logger.LogWarning("Slow database operation detected: {@DatabaseOperation}", dbOperation);
            else
                logger.LogDebug("Database operation completed: {@DatabaseOperation}", dbOperation);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to log database operation {Operation} on {TableName}", operation, tableName);
        }
    }

    public static void LogSecurityEvent(this MSLogger logger, string eventType, string? userId = null,
        string? ipAddress = null, bool success = true, string? details = null)
    {
        try
        {
            var securityEvent = new
            {
                EventType = eventType,
                UserId = userId,
                IPAddress = ipAddress,
                Success = success,
                Details = details,
                Timestamp = DateTimeOffset.UtcNow
            };

            if (!success)
                logger.LogWarning("Security event failed: {@SecurityEvent}", securityEvent);
            else
                logger.LogInformation("Security event recorded: {@SecurityEvent}", securityEvent);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to log security event {EventType}", eventType);
        }
    }

    public static void LogBusinessEvent(this MSLogger logger, string eventName, string? entityType = null,
        string? entityId = null, string? userId = null, object? eventData = null)
    {
        try
        {
            var businessEvent = new
            {
                EventName = eventName,
                EntityType = entityType,
                EntityId = entityId,
                UserId = userId,
                EventData = eventData,
                Timestamp = DateTimeOffset.UtcNow
            };

            logger.LogInformation("Business event recorded: {@BusinessEvent}", businessEvent);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to log business event {EventName}", eventName);
        }
    }

    public static void LogApiRequest(this MSLogger logger, HttpContext context, long elapsedMs,
        int statusCode, long? responseSize = null)
    {
        try
        {
            var apiRequest = new
            {
                context.Request.Method,
                Path = context.Request.Path.Value,
                QueryString = context.Request.QueryString.Value,
                StatusCode = statusCode,
                ElapsedMs = elapsedMs,
                ResponseSizeBytes = responseSize,
                UserAgent = context.Request.Headers.UserAgent.ToString(),
                IPAddress = context.Connection.RemoteIpAddress?.ToString(),
                CorrelationId = context.GetOrCreateCorrelationId(),
                Timestamp = DateTimeOffset.UtcNow
            };

            var logLevel = statusCode switch
            {
                >= 500 => LogLevel.Error,
                >= 400 => LogLevel.Warning,
                _ when elapsedMs > 2000 => LogLevel.Warning,
                _ => LogLevel.Information
            };

            logger.Log(logLevel, "API request completed: {@ApiRequest}", apiRequest);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to log API request");
        }
    }

    public static IDisposable CreateLogScope(this MSLogger logger, params (string Key, object Value)[] properties)
    {
        var disposables = new List<IDisposable>();

        foreach (var (key, value) in properties) disposables.Add(LogContext.PushProperty(key, value));

        return new CompositeDisposable(disposables);
    }

    public static void LogAggregatedMetrics(this MSLogger logger, string metricName,
        IEnumerable<double> values, string? unit = null)
    {
        try
        {
            var valueList = values.ToList();
            if (!valueList.Any()) return;

            var aggregatedMetrics = new
            {
                MetricName = metricName,
                Unit = unit,
                valueList.Count,
                Sum = valueList.Sum(),
                Average = valueList.Average(),
                Min = valueList.Min(),
                Max = valueList.Max(),
                Median = GetMedian(valueList),
                StandardDeviation = GetStandardDeviation(valueList),
                Timestamp = DateTimeOffset.UtcNow
            };

            logger.LogInformation("Aggregated metrics: {@AggregatedMetrics}", aggregatedMetrics);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to log aggregated metrics for {MetricName}", metricName);
        }
    }

    private static double GetMedian(List<double> values)
    {
        var sorted = values.OrderBy(x => x).ToList();
        var count = sorted.Count;

        if (count % 2 == 0) return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;

        return sorted[count / 2];
    }

    private static double GetStandardDeviation(List<double> values)
    {
        var avg = values.Average();
        var sumOfSquares = values.Select(x => Math.Pow(x - avg, 2)).Sum();
        return Math.Sqrt(sumOfSquares / values.Count);
    }

    public static void ConfigureLogRetention(this LoggerConfiguration loggerConfig, IConfiguration configuration)
    {
        var retentionOptions = configuration.GetSection("Logging:Retention").Get<LogRetentionOptions>()
                               ?? new LogRetentionOptions();

        // Configure file retention policies
        loggerConfig
            .WriteTo.File(
                retentionOptions.LogFilePath,
                rollingInterval: retentionOptions.RollingInterval,
                retainedFileCountLimit: retentionOptions.RetainedFileCount,
                fileSizeLimitBytes: retentionOptions.FileSizeLimitBytes,
                rollOnFileSizeLimit: true,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(retentionOptions.FlushIntervalSeconds));
    }

    // Performance monitoring extensions using Serilog directly
    public static void LogPerformanceCounter(this SerilogLogger logger, string counterName, double value,
        string? unit = null, string? category = null)
    {
        logger.Information("Performance counter {CounterName} in category {Category}: {Value} {Unit}",
            counterName, category ?? "General", value, unit ?? "units");
    }

    public static void LogResourceUsage(this SerilogLogger logger, string resourceType, double usage,
        double capacity, string? unit = null)
    {
        var utilizationPercent = capacity > 0 ? usage / capacity * 100 : 0;

        var resourceMetrics = new
        {
            ResourceType = resourceType,
            Usage = usage,
            Capacity = capacity,
            UtilizationPercent = Math.Round(utilizationPercent, 2),
            Unit = unit ?? "units",
            Timestamp = DateTimeOffset.UtcNow
        };

        var logLevel = utilizationPercent switch
        {
            > 90 => LogEventLevel.Error,
            > 80 => LogEventLevel.Warning,
            _ => LogEventLevel.Information
        };

        logger.Write(logLevel, "Resource usage: {@ResourceMetrics}", resourceMetrics);
    }
}

internal class OperationScope : IDisposable
{
    private readonly string _correlationId;
    private readonly MSLogger _logger;
    private readonly string _operationName;
    private readonly Stopwatch _stopwatch;
    private bool _disposed;

    public OperationScope(MSLogger logger, string operationName, Stopwatch stopwatch, string correlationId)
    {
        _logger = logger;
        _operationName = operationName;
        _stopwatch = stopwatch;
        _correlationId = correlationId;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _stopwatch.Stop();
            _logger.LogMethodExit(_operationName, null, _stopwatch.ElapsedMilliseconds);
            _disposed = true;
        }
    }
}

public class LogRetentionOptions
{
    public string LogFilePath { get; set; } = "logs/application-.log";
    public RollingInterval RollingInterval { get; set; } = RollingInterval.Day;
    public int RetainedFileCount { get; set; } = 31;
    public long FileSizeLimitBytes { get; set; } = 100 * 1024 * 1024; // 100MB
    public int FlushIntervalSeconds { get; set; } = 10;
}

internal class CompositeDisposable : IDisposable
{
    private readonly List<IDisposable> _disposables;
    private bool _disposed;

    public CompositeDisposable(List<IDisposable> disposables)
    {
        _disposables = disposables ?? throw new ArgumentNullException(nameof(disposables));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            foreach (var disposable in _disposables) disposable?.Dispose();
            _disposed = true;
        }
    }
}