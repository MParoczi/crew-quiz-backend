using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Backend.Interfaces.Services;

namespace Backend.Services;

public class LoggingMetricsService : ILoggingMetricsService
{
    private readonly IConfiguration _configuration;
    private readonly ConcurrentDictionary<string, long> _counters;
    private readonly ILogger<LoggingMetricsService> _logger;
    private readonly Timer _metricsReportingTimer;
    private readonly ConcurrentDictionary<string, OperationMetrics> _operationMetrics;
    private readonly LoggingMetricsOptions _options;
    private DateTime _startTime;

    public LoggingMetricsService(
        ILogger<LoggingMetricsService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _operationMetrics = new ConcurrentDictionary<string, OperationMetrics>();
        _counters = new ConcurrentDictionary<string, long>();
        _startTime = DateTime.UtcNow;

        _options = configuration.GetSection("LoggingMetrics").Get<LoggingMetricsOptions>()
                   ?? new LoggingMetricsOptions();

        // Set up periodic metrics reporting
        _metricsReportingTimer = new Timer(ReportMetrics, null,
            TimeSpan.FromMinutes(_options.ReportingIntervalMinutes),
            TimeSpan.FromMinutes(_options.ReportingIntervalMinutes));

        _logger.LogInformation("LoggingMetricsService initialized with reporting interval of {IntervalMinutes} minutes",
            _options.ReportingIntervalMinutes);
    }

    public void RecordOperationMetrics(string operationName, long durationMs, bool success = true,
        string? additionalInfo = null)
    {
        try
        {
            var metrics = _operationMetrics.AddOrUpdate(operationName,
                new OperationMetrics(operationName),
                (key, existing) => existing);

            metrics.AddExecution(durationMs, success, additionalInfo);

            // Log slow operations immediately
            if (durationMs > _options.SlowOperationThresholdMs)
                _logger.LogWarning("Slow operation detected: {OperationName} took {DurationMs}ms. Success: {Success}. Info: {AdditionalInfo}",
                    operationName, durationMs, success, additionalInfo);

            // Log failed operations immediately
            if (!success)
                _logger.LogError("Operation failed: {OperationName} took {DurationMs}ms. Info: {AdditionalInfo}",
                    operationName, durationMs, additionalInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record operation metrics for {OperationName}", operationName);
        }
    }

    public void IncrementCounter(string counterName, long value = 1)
    {
        try
        {
            _counters.AddOrUpdate(counterName, value, (key, existing) => existing + value);

            _logger.LogDebug("Counter {CounterName} incremented by {Value}. New total: {Total}",
                counterName, value, _counters[counterName]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to increment counter {CounterName}", counterName);
        }
    }

    public void RecordMemoryUsage(string context = "General")
    {
        try
        {
            var memoryBefore = GC.GetTotalMemory(false);
            GC.Collect();
            var memoryAfter = GC.GetTotalMemory(true);

            var memoryMetrics = new MemoryMetrics
            {
                Context = context,
                TotalMemoryBytes = memoryAfter,
                CollectedBytes = memoryBefore - memoryAfter,
                Gen0Collections = GC.CollectionCount(0),
                Gen1Collections = GC.CollectionCount(1),
                Gen2Collections = GC.CollectionCount(2),
                Timestamp = DateTimeOffset.UtcNow
            };

            _logger.LogInformation("Memory usage recorded for {Context}: {@MemoryMetrics}",
                context, memoryMetrics);

            // Alert if memory usage is high
            var memoryMB = memoryAfter / 1024 / 1024;
            if (memoryMB > _options.HighMemoryThresholdMB)
                _logger.LogWarning("High memory usage detected in {Context}: {MemoryMB}MB (threshold: {ThresholdMB}MB)",
                    context, memoryMB, _options.HighMemoryThresholdMB);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record memory usage for context {Context}", context);
        }
    }

    public void RecordSystemPerformance()
    {
        try
        {
            var process = Process.GetCurrentProcess();

            var performanceMetrics = new SystemPerformanceMetrics
            {
                ProcessId = process.Id,
                ProcessName = process.ProcessName,
                WorkingSetMB = process.WorkingSet64 / 1024 / 1024,
                CpuTimeMs = process.TotalProcessorTime.TotalMilliseconds,
                ThreadCount = process.Threads.Count,
                HandleCount = process.HandleCount,
                UptimeSeconds = (DateTime.UtcNow - _startTime).TotalSeconds,
                ProcessorCount = Environment.ProcessorCount,
                GCTotalMemoryMB = GC.GetTotalMemory(false) / 1024 / 1024,
                ThreadPoolWorkerThreads = ThreadPool.ThreadCount,
                ThreadPoolCompletedWorkItems = ThreadPool.CompletedWorkItemCount,
                Timestamp = DateTimeOffset.UtcNow
            };

            _logger.LogInformation("System performance metrics recorded: {@SystemPerformanceMetrics}",
                performanceMetrics);

            // Check for performance issues
            CheckPerformanceThresholds(performanceMetrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record system performance metrics");
        }
    }

    public MetricsSummary GetMetricsSummary()
    {
        try
        {
            var summary = new MetricsSummary
            {
                StartTime = _startTime,
                CurrentTime = DateTime.UtcNow,
                UptimeHours = (DateTime.UtcNow - _startTime).TotalHours,
                OperationMetrics = _operationMetrics.Values.Select(m => m.GetSummary()).ToList(),
                Counters = new Dictionary<string, long>(_counters),
                SystemInfo = GetCurrentSystemInfo()
            };

            _logger.LogDebug("Metrics summary generated with {OperationCount} operations and {CounterCount} counters",
                summary.OperationMetrics.Count, summary.Counters.Count);

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate metrics summary");
            return new MetricsSummary
            {
                StartTime = _startTime,
                CurrentTime = DateTime.UtcNow,
                OperationMetrics = [],
                Counters = new Dictionary<string, long>(),
                SystemInfo = new SystemInfo()
            };
        }
    }

    public void ResetMetrics()
    {
        try
        {
            var operationCount = _operationMetrics.Count;
            var counterCount = _counters.Count;

            _operationMetrics.Clear();
            _counters.Clear();
            _startTime = DateTime.UtcNow;

            _logger.LogInformation("Metrics reset completed. Cleared {OperationCount} operations and {CounterCount} counters",
                operationCount, counterCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset metrics");
        }
    }

    public void ExportMetrics(string exportPath, MetricsExportFormat format = MetricsExportFormat.Json)
    {
        try
        {
            var summary = GetMetricsSummary();
            var exportData = format switch
            {
                MetricsExportFormat.Json => JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true }),
                MetricsExportFormat.Csv => ConvertToCsv(summary),
                _ => throw new ArgumentOutOfRangeException(nameof(format))
            };

            File.WriteAllText(exportPath, exportData);

            _logger.LogInformation("Metrics exported to {ExportPath} in {Format} format. File size: {FileSize} bytes",
                exportPath, format, new FileInfo(exportPath).Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export metrics to {ExportPath} in {Format} format", exportPath, format);
        }
    }

    public void Dispose()
    {
        try
        {
            _logger.LogInformation("Disposing LoggingMetricsService");

            _metricsReportingTimer?.Dispose();

            // Final metrics report
            ReportMetrics(null);

            _logger.LogInformation("LoggingMetricsService disposed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during LoggingMetricsService disposal");
        }
    }

    private void ReportMetrics(object? state)
    {
        try
        {
            _logger.LogInformation("Starting periodic metrics reporting");

            var summary = GetMetricsSummary();

            _logger.LogInformation("Periodic metrics report: {@MetricsSummary}", summary);

            // Record current system performance
            RecordSystemPerformance();
            RecordMemoryUsage("PeriodicReport");

            // Log top slow operations
            var slowOperations = summary.OperationMetrics
                .Where(m => m.AverageExecutionTimeMs > _options.SlowOperationThresholdMs)
                .OrderByDescending(m => m.AverageExecutionTimeMs)
                .Take(5)
                .ToList();

            if (slowOperations.Any()) _logger.LogWarning("Top slow operations in this period: {@SlowOperations}", slowOperations);

            // Log error-prone operations
            var errorOperations = summary.OperationMetrics
                .Where(m => m.ErrorRate > 0.1) // More than 10% error rate
                .OrderByDescending(m => m.ErrorRate)
                .Take(5)
                .ToList();

            if (errorOperations.Any()) _logger.LogError("Operations with high error rates: {@ErrorOperations}", errorOperations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during periodic metrics reporting");
        }
    }

    private void CheckPerformanceThresholds(SystemPerformanceMetrics metrics)
    {
        if (metrics.WorkingSetMB > _options.HighMemoryThresholdMB)
            _logger.LogWarning("High memory usage detected: {WorkingSetMB}MB (threshold: {ThresholdMB}MB)",
                metrics.WorkingSetMB, _options.HighMemoryThresholdMB);

        if (metrics.ThreadCount > _options.HighThreadCountThreshold)
            _logger.LogWarning("High thread count detected: {ThreadCount} (threshold: {ThresholdCount})",
                metrics.ThreadCount, _options.HighThreadCountThreshold);

        if (metrics.HandleCount > _options.HighHandleCountThreshold)
            _logger.LogWarning("High handle count detected: {HandleCount} (threshold: {ThresholdCount})",
                metrics.HandleCount, _options.HighHandleCountThreshold);
    }

    private SystemInfo GetCurrentSystemInfo()
    {
        var process = Process.GetCurrentProcess();

        return new SystemInfo
        {
            ProcessorCount = Environment.ProcessorCount,
            MachineName = Environment.MachineName,
            OSVersion = Environment.OSVersion.ToString(),
            WorkingSetMB = Environment.WorkingSet / 1024 / 1024,
            ProcessName = process.ProcessName,
            ProcessId = process.Id,
            Timestamp = DateTimeOffset.UtcNow
        };
    }

    private string ConvertToCsv(MetricsSummary summary)
    {
        var csv = new StringBuilder();

        // Headers
        csv.AppendLine("Metric,Value");
        csv.AppendLine($"StartTime,{summary.StartTime:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine($"CurrentTime,{summary.CurrentTime:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine($"UptimeHours,{summary.UptimeHours:F2}");

        // Operation metrics
        csv.AppendLine("Operation,ExecutionCount,AverageTimeMs,MaxTimeMs,MinTimeMs,ErrorRate");
        foreach (var op in summary.OperationMetrics)
            csv.AppendLine(
                $"{op.OperationName},{op.ExecutionCount},{op.AverageExecutionTimeMs:F2},{op.MaxExecutionTimeMs},{op.MinExecutionTimeMs},{op.ErrorRate:F3}");

        // Counters
        csv.AppendLine("Counter,Value");
        foreach (var counter in summary.Counters) csv.AppendLine($"{counter.Key},{counter.Value}");

        return csv.ToString();
    }
}

// Supporting classes for metrics
public class OperationMetrics
{
    private readonly List<ExecutionRecord> _executions = [];
    private readonly object _lock = new();

    public OperationMetrics(string operationName)
    {
        OperationName = operationName;
    }

    public string OperationName { get; }

    public void AddExecution(long durationMs, bool success, string? additionalInfo = null)
    {
        lock (_lock)
        {
            _executions.Add(new ExecutionRecord
            {
                DurationMs = durationMs,
                Success = success,
                Timestamp = DateTimeOffset.UtcNow,
                AdditionalInfo = additionalInfo
            });
        }
    }

    public OperationMetricsSummary GetSummary()
    {
        lock (_lock)
        {
            if (_executions.Count == 0)
                return new OperationMetricsSummary
                {
                    OperationName = OperationName,
                    ExecutionCount = 0,
                    AverageExecutionTimeMs = 0,
                    MaxExecutionTimeMs = 0,
                    MinExecutionTimeMs = 0,
                    ErrorRate = 0
                };

            var durations = _executions.Select(e => e.DurationMs).ToList();
            var errorCount = _executions.Count(e => !e.Success);

            return new OperationMetricsSummary
            {
                OperationName = OperationName,
                ExecutionCount = _executions.Count,
                AverageExecutionTimeMs = durations.Average(),
                MaxExecutionTimeMs = durations.Max(),
                MinExecutionTimeMs = durations.Min(),
                ErrorRate = (double)errorCount / _executions.Count
            };
        }
    }
}

public class ExecutionRecord
{
    public long DurationMs { get; set; }
    public bool Success { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string? AdditionalInfo { get; set; }
}

public class OperationMetricsSummary
{
    public string OperationName { get; set; } = string.Empty;
    public int ExecutionCount { get; set; }
    public double AverageExecutionTimeMs { get; set; }
    public long MaxExecutionTimeMs { get; set; }
    public long MinExecutionTimeMs { get; set; }
    public double ErrorRate { get; set; }
}

public class MemoryMetrics
{
    public string Context { get; set; } = string.Empty;
    public long TotalMemoryBytes { get; set; }
    public long CollectedBytes { get; set; }
    public int Gen0Collections { get; set; }
    public int Gen1Collections { get; set; }
    public int Gen2Collections { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

public class SystemPerformanceMetrics
{
    public int ProcessId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public long WorkingSetMB { get; set; }
    public double CpuTimeMs { get; set; }
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
    public double UptimeSeconds { get; set; }
    public int ProcessorCount { get; set; }
    public long GCTotalMemoryMB { get; set; }
    public int ThreadPoolWorkerThreads { get; set; }
    public long ThreadPoolCompletedWorkItems { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

public class MetricsSummary
{
    public DateTime StartTime { get; set; }
    public DateTime CurrentTime { get; set; }
    public double UptimeHours { get; set; }
    public List<OperationMetricsSummary> OperationMetrics { get; set; } = [];
    public Dictionary<string, long> Counters { get; set; } = new();
    public SystemInfo SystemInfo { get; set; } = new();
}

public class SystemInfo
{
    public int ProcessorCount { get; set; }
    public string MachineName { get; set; } = string.Empty;
    public string OSVersion { get; set; } = string.Empty;
    public long WorkingSetMB { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public int ProcessId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

public class LoggingMetricsOptions
{
    public int ReportingIntervalMinutes { get; set; } = 15;
    public int SlowOperationThresholdMs { get; set; } = 1000;
    public int HighMemoryThresholdMB { get; set; } = 512;
    public int HighThreadCountThreshold { get; set; } = 100;
    public int HighHandleCountThreshold { get; set; } = 1000;
}

public enum MetricsExportFormat
{
    Json,
    Csv
}