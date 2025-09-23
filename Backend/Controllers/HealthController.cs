using System.Diagnostics;
using System.Reflection;
using Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly CrewQuizContext _context;

    public HealthController(
        CrewQuizContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> GetHealth()
    {
        var healthStatus = await GetBasicHealthStatusAsync();
        return Ok(healthStatus);
    }

    [HttpGet("detailed")]
    public async Task<IActionResult> GetDetailedHealth()
    {
        var healthStatus = await GetDetailedHealthStatusAsync();
        return healthStatus.OverallStatus == "Healthy" ? Ok(healthStatus) : StatusCode(503, healthStatus);
    }

    [HttpGet("database")]
    public async Task<IActionResult> GetDatabaseHealth()
    {
        var dbHealth = await CheckDatabaseHealthAsync();
        return dbHealth.Status == "Healthy" ? Ok(dbHealth) : StatusCode(503, dbHealth);
    }

    [HttpGet("metrics")]
    [Authorize]
    public IActionResult GetMetrics()
    {
        var metrics = GetSystemMetricsData();
        return Ok(metrics);
    }

    private async Task<BasicHealthStatus> GetBasicHealthStatusAsync()
    {
        var dbHealthy = await IsDatabaseHealthyAsync();

        return new BasicHealthStatus
        {
            Status = dbHealthy ? "Healthy" : "Unhealthy",
            Timestamp = DateTimeOffset.UtcNow,
            Version = GetApplicationVersion(),
            Environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Unknown"
        };
    }

    private async Task<DetailedHealthStatus> GetDetailedHealthStatusAsync()
    {
        var dbHealthTask = CheckDatabaseHealthAsync();
        var systemHealthTask = Task.FromResult(CheckSystemHealth());
        var appHealthTask = Task.FromResult(CheckApplicationHealth());

        await Task.WhenAll(dbHealthTask, systemHealthTask, appHealthTask);

        var dbHealth = await dbHealthTask;
        var systemHealth = await systemHealthTask;
        var appHealth = await appHealthTask;

        var overallStatus = DetermineOverallStatus(dbHealth.Status, systemHealth.Status, appHealth.Status);

        return new DetailedHealthStatus
        {
            OverallStatus = overallStatus,
            Timestamp = DateTimeOffset.UtcNow,
            Database = dbHealth,
            System = systemHealth,
            Application = appHealth,
            Checks = new HealthChecks
            {
                DatabaseConnectivity = dbHealth.Status,
                SystemResources = systemHealth.Status,
                ApplicationComponents = appHealth.Status
            }
        };
    }

    private async Task<DatabaseHealth> CheckDatabaseHealthAsync()
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Test basic connectivity
            var canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                stopwatch.Stop();

                return new DatabaseHealth
                {
                    Status = "Unhealthy",
                    ConnectionStatus = "Failed",
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                    Error = "Cannot connect to database"
                };
            }

            // Test query performance
            var userCount = await _context.User.CountAsync();
            stopwatch.Stop();

            var responseTime = stopwatch.ElapsedMilliseconds;
            var status = responseTime < 1000 ? "Healthy" : "Degraded";

            return new DatabaseHealth
            {
                Status = status,
                ConnectionStatus = "Connected",
                ResponseTimeMs = responseTime,
                ActiveConnections = GetActiveConnectionCount(),
                RecordCounts = new RecordCounts
                {
                    Users = userCount,
                    Timestamp = DateTimeOffset.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            return new DatabaseHealth
            {
                Status = "Unhealthy",
                ConnectionStatus = "Error",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                Error = ex.Message
            };
        }
    }

    private SystemHealth CheckSystemHealth()
    {
        try
        {
            var process = Process.GetCurrentProcess();
            var memoryMB = process.WorkingSet64 / 1024 / 1024;
            var uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime();

            return new SystemHealth
            {
                Status = memoryMB < 1024 ? "Healthy" : "Warning", // Warning if over 1GB
                MemoryUsageMB = memoryMB,
                CpuUsagePercent = GetCpuUsage(),
                UptimeHours = Math.Round(uptime.TotalHours, 2),
                ThreadCount = process.Threads.Count,
                GarbageCollector = new GarbageCollectorInfo
                {
                    Gen0Collections = GC.CollectionCount(0),
                    Gen1Collections = GC.CollectionCount(1),
                    Gen2Collections = GC.CollectionCount(2),
                    TotalMemoryMB = GC.GetTotalMemory(false) / 1024 / 1024
                }
            };
        }
        catch (Exception ex)
        {
            return new SystemHealth
            {
                Status = "Unhealthy",
                Error = ex.Message
            };
        }
    }

    private ApplicationHealth CheckApplicationHealth()
    {
        try
        {
            return new ApplicationHealth
            {
                Status = "Healthy",
                Version = GetApplicationVersion(),
                Environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Unknown",
                BuildDate = GetBuildDate(),
                Features = new ApplicationFeatures
                {
                    SignalREnabled = true,
                    AuthenticationEnabled = true,
                    DatabaseMigrationsApplied = CheckMigrationsStatus()
                }
            };
        }
        catch (Exception ex)
        {
            return new ApplicationHealth
            {
                Status = "Unhealthy",
                Error = ex.Message
            };
        }
    }

    private object GetSystemMetricsData()
    {
        var process = Process.GetCurrentProcess();

        return new
        {
            Timestamp = DateTimeOffset.UtcNow,
            Process = new
            {
                process.Id,
                Name = process.ProcessName,
                MemoryUsageMB = process.WorkingSet64 / 1024 / 1024,
                CpuTimeMs = process.TotalProcessorTime.TotalMilliseconds,
                ThreadCount = process.Threads.Count,
                process.HandleCount
            },
            System = new
            {
                Environment.ProcessorCount,
                Environment.MachineName,
                OSVersion = Environment.OSVersion.ToString(),
                Is64BitOS = Environment.Is64BitOperatingSystem,
                WorkingSetMB = Environment.WorkingSet / 1024 / 1024
            },
            Runtime = new
            {
                Version = Environment.Version.ToString(),
                GCMemoryMB = GC.GetTotalMemory(false) / 1024 / 1024,
                GCGen0Collections = GC.CollectionCount(0),
                GCGen1Collections = GC.CollectionCount(1),
                GCGen2Collections = GC.CollectionCount(2)
            },
            ThreadPool = new
            {
                WorkerThreads = ThreadPool.ThreadCount,
                CompletedWorkItems = ThreadPool.CompletedWorkItemCount
            }
        };
    }

    private async Task<bool> IsDatabaseHealthyAsync()
    {
        try
        {
            return await _context.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    private string DetermineOverallStatus(string dbStatus, string systemStatus, string appStatus)
    {
        var statuses = new[] { dbStatus, systemStatus, appStatus };

        if (statuses.Any(s => s == "Unhealthy"))
            return "Unhealthy";

        if (statuses.Any(s => s == "Degraded" || s == "Warning"))
            return "Degraded";

        return "Healthy";
    }

    private string GetApplicationVersion()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version?.ToString() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    private string GetBuildDate()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var creationTime = System.IO.File.GetCreationTime(assembly.Location);
            return creationTime.ToString("yyyy-MM-dd HH:mm:ss UTC");
        }
        catch
        {
            return "Unknown";
        }
    }

    private bool CheckMigrationsStatus()
    {
        try
        {
            var pendingMigrations = _context.Database.GetPendingMigrations();
            return !pendingMigrations.Any();
        }
        catch
        {
            return false;
        }
    }

    private int GetActiveConnectionCount()
    {
        try
        {
            // This is a simplified approach. In production, you might want to use
            // database-specific queries to get actual connection pool information
            return 1; // Placeholder - would need database-specific implementation
        }
        catch
        {
            return 0;
        }
    }

    private double GetCpuUsage()
    {
        try
        {
            // This is a simplified CPU usage calculation
            // In production, you might want to use performance counters or other methods
            var process = Process.GetCurrentProcess();
            return Math.Round(process.TotalProcessorTime.TotalMilliseconds / Environment.TickCount * 100, 2);
        }
        catch
        {
            return 0.0;
        }
    }
}

// Health status models
public class BasicHealthStatus
{
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
}

public class DetailedHealthStatus
{
    public string OverallStatus { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public DatabaseHealth Database { get; set; } = new();
    public SystemHealth System { get; set; } = new();
    public ApplicationHealth Application { get; set; } = new();
    public HealthChecks Checks { get; set; } = new();
}

public class DatabaseHealth
{
    public string Status { get; set; } = string.Empty;
    public string ConnectionStatus { get; set; } = string.Empty;
    public long ResponseTimeMs { get; set; }
    public int ActiveConnections { get; set; }
    public RecordCounts? RecordCounts { get; set; }
    public string? Error { get; set; }
}

public class SystemHealth
{
    public string Status { get; set; } = string.Empty;
    public long MemoryUsageMB { get; set; }
    public double CpuUsagePercent { get; set; }
    public double UptimeHours { get; set; }
    public int ThreadCount { get; set; }
    public GarbageCollectorInfo GarbageCollector { get; set; } = new();
    public string? Error { get; set; }
}

public class ApplicationHealth
{
    public string Status { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string BuildDate { get; set; } = string.Empty;
    public ApplicationFeatures Features { get; set; } = new();
    public string? Error { get; set; }
}

public class HealthChecks
{
    public string DatabaseConnectivity { get; set; } = string.Empty;
    public string SystemResources { get; set; } = string.Empty;
    public string ApplicationComponents { get; set; } = string.Empty;
}

public class RecordCounts
{
    public int Users { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

public class GarbageCollectorInfo
{
    public int Gen0Collections { get; set; }
    public int Gen1Collections { get; set; }
    public int Gen2Collections { get; set; }
    public long TotalMemoryMB { get; set; }
}

public class ApplicationFeatures
{
    public bool SignalREnabled { get; set; }
    public bool AuthenticationEnabled { get; set; }
    public bool DatabaseMigrationsApplied { get; set; }
}