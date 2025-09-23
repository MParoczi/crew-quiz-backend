using Backend.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CrewQuiz.Tests.DataManagement;

public class SessionCleanupTests : TestBase
{
    private readonly ISessionCleanupService _sessionCleanupService;

    public SessionCleanupTests()
    {
        _sessionCleanupService = ServiceProvider.GetRequiredService<ISessionCleanupService>();
    }

    [Fact]
    public async Task CleanupInactiveSessionsAsync_ShouldReturnValidResult()
    {
        // Act
        var result = await _sessionCleanupService.CleanupInactiveSessionsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(24, result.SessionTimeoutHours);
        Assert.True(result.ExecutionTime >= TimeSpan.Zero);

        Console.WriteLine($"[DEBUG_LOG] Cleanup test completed in {result.ExecutionTime.TotalMilliseconds}ms");
    }

    [Fact]
    public async Task GetSessionStatisticsAsync_ShouldReturnValidStatistics()
    {
        // Act
        var statistics = await _sessionCleanupService.GetSessionStatisticsAsync();

        // Assert
        Assert.NotNull(statistics);
        Assert.True(statistics.GeneratedAt > DateTime.MinValue);
        Assert.Equal(24, statistics.SessionTimeoutHours);
        Assert.True(statistics.TotalActiveSessions >= 0);

        Console.WriteLine($"[DEBUG_LOG] Statistics: Active sessions={statistics.TotalActiveSessions}, " +
                          $"Cleanup candidates={statistics.TotalCleanupCandidates}");
    }

    [Fact]
    public async Task PerformFullCleanupAsync_ShouldCompleteSuccessfully()
    {
        // Act
        var result = await _sessionCleanupService.PerformFullCleanupAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(24, result.SessionTimeoutHours);
        Assert.True(result.ExecutionTime >= TimeSpan.Zero);

        Console.WriteLine($"[DEBUG_LOG] Full cleanup completed. Records affected: {result.TotalRecordsAffected}");
    }
}