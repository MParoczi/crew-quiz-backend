using Backend.Services;

namespace Backend.Interfaces.Services;

public interface ILoggingMetricsService : IDisposable
{
    /// <summary>
    ///     Records metrics for an operation including duration and success status
    /// </summary>
    /// <param name="operationName">Name of the operation</param>
    /// <param name="durationMs">Duration in milliseconds</param>
    /// <param name="success">Whether the operation succeeded</param>
    /// <param name="additionalInfo">Additional contextual information</param>
    void RecordOperationMetrics(string operationName, long durationMs, bool success = true, string? additionalInfo = null);

    /// <summary>
    ///     Increments a named counter by the specified value
    /// </summary>
    /// <param name="counterName">Name of the counter</param>
    /// <param name="value">Value to increment by (default: 1)</param>
    void IncrementCounter(string counterName, long value = 1);

    /// <summary>
    ///     Records current memory usage with optional context
    /// </summary>
    /// <param name="context">Context for the memory measurement</param>
    void RecordMemoryUsage(string context = "General");

    /// <summary>
    ///     Records comprehensive system performance metrics
    /// </summary>
    void RecordSystemPerformance();

    /// <summary>
    ///     Gets a summary of all collected metrics
    /// </summary>
    /// <returns>Comprehensive metrics summary</returns>
    MetricsSummary GetMetricsSummary();

    /// <summary>
    ///     Resets all collected metrics
    /// </summary>
    void ResetMetrics();

    /// <summary>
    ///     Exports metrics to a file in the specified format
    /// </summary>
    /// <param name="exportPath">Path to export the metrics file</param>
    /// <param name="format">Export format (Json or Csv)</param>
    void ExportMetrics(string exportPath, MetricsExportFormat format = MetricsExportFormat.Json);
}