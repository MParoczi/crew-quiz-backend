using System.Diagnostics;
using Backend.Interfaces.Data;
using Backend.Interfaces.Services;
using Backend.Models.DTOs;

namespace Backend.Services;

public class SessionCleanupService(
    IUnitOfWork unitOfWork,
    ILogger<SessionCleanupService> logger) : ISessionCleanupService
{
    public async Task<SessionCleanupResultDto> CleanupInactiveSessionsAsync(int sessionTimeoutHours = 24)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new SessionCleanupResultDto
        {
            CleanupTimestamp = DateTime.UtcNow,
            SessionTimeoutHours = sessionTimeoutHours,
            IsSuccess = true
        };

        try
        {
            var timeoutThreshold = DateTime.UtcNow.AddHours(-sessionTimeoutHours);

            // Find inactive sessions (not completed and haven't been updated within timeout period)
            var allSessions = await unitOfWork.CurrentGames.GetAllAsync();
            var inactiveSessions = allSessions.Where(cg => !cg.IsCompleted &&
                                                           (cg.UpdatedOn == null ? cg.CreatedOn : cg.UpdatedOn.Value) < timeoutThreshold).ToList();

            foreach (var session in inactiveSessions)
                try
                {
                    // Remove related CurrentGameUsers
                    var allGameUsers = await unitOfWork.CurrentGameUsers.GetAllAsync();
                    var gameUsers = allGameUsers.Where(cgu => cgu.CurrentGameId == session.CurrentGameId).ToList();

                    foreach (var gameUser in gameUsers)
                    {
                        await unitOfWork.CurrentGameUsers.RemoveAsync(gameUser);
                        result.CleanedUpGameUsers++;
                    }

                    // Remove related CurrentGameQuestions
                    var allGameQuestions = await unitOfWork.CurrentGameQuestions.GetAllAsync();
                    var gameQuestions = allGameQuestions.Where(cgq => cgq.CurrentGameId == session.CurrentGameId).ToList();

                    foreach (var gameQuestion in gameQuestions)
                    {
                        await unitOfWork.CurrentGameQuestions.RemoveAsync(gameQuestion);
                        result.CleanedUpGameQuestions++;
                    }

                    // Remove the session itself
                    await unitOfWork.CurrentGames.RemoveAsync(session);
                    result.CleanedUpSessions++;
                }
                catch (Exception ex)
                {
                    var error = $"Failed to clean up session {session.SessionId}: {ex.Message}";
                    result.Errors.Add(error);
                    result.IsSuccess = false;
                }

            // Save all changes in a single transaction
            if (result.TotalRecordsAffected > 0) await unitOfWork.CompleteAsync();
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Errors.Add($"Cleanup operation failed: {ex.Message}");
        }
        finally
        {
            stopwatch.Stop();
            result.ExecutionTime = stopwatch.Elapsed;
        }

        return result;
    }

    public async Task<SessionCleanupResultDto> CleanupOrphanedDataAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new SessionCleanupResultDto
        {
            CleanupTimestamp = DateTime.UtcNow,
            SessionTimeoutHours = 0, // Not applicable for orphaned data cleanup
            IsSuccess = true
        };

        try
        {
            // Find orphaned CurrentGameUsers (users linked to non-existent games)
            var allGameUsers = await unitOfWork.CurrentGameUsers.GetAllAsync();
            var allGameIds = (await unitOfWork.CurrentGames.GetAllAsync()).Select(cg => cg.CurrentGameId).ToHashSet();

            var orphanedGameUsers = allGameUsers.Where(cgu => !allGameIds.Contains(cgu.CurrentGameId)).ToList();

            foreach (var orphanedUser in orphanedGameUsers)
                try
                {
                    await unitOfWork.CurrentGameUsers.RemoveAsync(orphanedUser);
                    result.CleanedUpGameUsers++;
                }
                catch (Exception ex)
                {
                    var error = $"Failed to delete orphaned game user (GameId: {orphanedUser.CurrentGameId}, UserId: {orphanedUser.UserId}): {ex.Message}";
                    result.Errors.Add(error);
                    result.IsSuccess = false;
                }

            // Find orphaned CurrentGameQuestions (questions linked to non-existent games)
            var allGameQuestions = await unitOfWork.CurrentGameQuestions.GetAllAsync();
            var orphanedGameQuestions = allGameQuestions.Where(cgq => !allGameIds.Contains(cgq.CurrentGameId)).ToList();

            foreach (var orphanedQuestion in orphanedGameQuestions)
                try
                {
                    await unitOfWork.CurrentGameQuestions.RemoveAsync(orphanedQuestion);
                    result.CleanedUpGameQuestions++;
                }
                catch (Exception ex)
                {
                    var error =
                        $"Failed to delete orphaned game question (GameId: {orphanedQuestion.CurrentGameId}, QuestionId: {orphanedQuestion.QuestionId}): {ex.Message}";
                    result.Errors.Add(error);
                    result.IsSuccess = false;
                }

            // Save all changes in a single transaction
            if (result.TotalRecordsAffected > 0) await unitOfWork.CompleteAsync();
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Errors.Add($"Orphaned data cleanup failed: {ex.Message}");
        }
        finally
        {
            stopwatch.Stop();
            result.ExecutionTime = stopwatch.Elapsed;
        }

        return result;
    }

    public async Task<SessionCleanupResultDto> PerformFullCleanupAsync(int sessionTimeoutHours = 24)
    {
        var stopwatch = Stopwatch.StartNew();

        var inactiveSessionsResult = await CleanupInactiveSessionsAsync(sessionTimeoutHours);
        var orphanedDataResult = await CleanupOrphanedDataAsync();

        var combinedResult = new SessionCleanupResultDto
        {
            CleanupTimestamp = DateTime.UtcNow,
            SessionTimeoutHours = sessionTimeoutHours,
            CleanedUpSessions = inactiveSessionsResult.CleanedUpSessions,
            CleanedUpGameUsers = inactiveSessionsResult.CleanedUpGameUsers + orphanedDataResult.CleanedUpGameUsers,
            CleanedUpGameQuestions = inactiveSessionsResult.CleanedUpGameQuestions + orphanedDataResult.CleanedUpGameQuestions,
            ExecutionTime = stopwatch.Elapsed,
            IsSuccess = inactiveSessionsResult.IsSuccess && orphanedDataResult.IsSuccess
        };

        combinedResult.Errors.AddRange(inactiveSessionsResult.Errors);
        combinedResult.Errors.AddRange(orphanedDataResult.Errors);

        stopwatch.Stop();
        combinedResult.ExecutionTime = stopwatch.Elapsed;

        return combinedResult;
    }

    public async Task<SessionStatisticsDto> GetSessionStatisticsAsync()
    {
        var stopwatch = Stopwatch.StartNew();

        var allSessions = await unitOfWork.CurrentGames.GetAllAsync();
        var sessionsList = allSessions.ToList();

        var statistics = new SessionStatisticsDto
        {
            GeneratedAt = DateTime.UtcNow,
            SessionTimeoutHours = 24, // Default timeout for statistics
            TotalActiveSessions = sessionsList.Count(s => !s.IsCompleted),
            InProgressSessions = sessionsList.Count(s => s.IsStarted && !s.IsCompleted),
            WaitingSessions = sessionsList.Count(s => !s.IsStarted && !s.IsCompleted),
            CompletedSessions = sessionsList.Count(s => s.IsCompleted),
            OldestActiveSession = sessionsList.Where(s => !s.IsCompleted).DefaultIfEmpty().Min(s => s?.CreatedOn),
            MostRecentActivity = sessionsList.DefaultIfEmpty().Max(s => s?.UpdatedOn ?? s?.CreatedOn)
        };

        // Calculate potentially inactive sessions (24 hour default)
        var timeoutThreshold = DateTime.UtcNow.AddHours(-statistics.SessionTimeoutHours);
        statistics.PotentiallyInactiveSessions = sessionsList.Count(s =>
            !s.IsCompleted &&
            (s.UpdatedOn == null ? s.CreatedOn : s.UpdatedOn.Value) < timeoutThreshold);

        // Calculate orphaned data
        var allGameUsers = await unitOfWork.CurrentGameUsers.GetAllAsync();
        var allGameQuestions = await unitOfWork.CurrentGameQuestions.GetAllAsync();
        var activeGameIds = sessionsList.Select(s => s.CurrentGameId).ToHashSet();

        statistics.OrphanedGameUsers = allGameUsers.Count(gu => !activeGameIds.Contains(gu.CurrentGameId));
        statistics.OrphanedGameQuestions = allGameQuestions.Count(gq => !activeGameIds.Contains(gq.CurrentGameId));

        stopwatch.Stop();

        return statistics;
    }
}