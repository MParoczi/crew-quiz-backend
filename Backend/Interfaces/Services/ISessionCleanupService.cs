using Backend.Models.DTOs;

namespace Backend.Interfaces.Services;

public interface ISessionCleanupService
{
    Task<SessionCleanupResultDto> CleanupInactiveSessionsAsync(int sessionTimeoutHours = 24);
    Task<SessionCleanupResultDto> CleanupOrphanedDataAsync();
    Task<SessionCleanupResultDto> PerformFullCleanupAsync(int sessionTimeoutHours = 24);
    Task<SessionStatisticsDto> GetSessionStatisticsAsync();
}