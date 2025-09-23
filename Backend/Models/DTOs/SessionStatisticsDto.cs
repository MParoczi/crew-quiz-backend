namespace Backend.Models.DTOs;

public class SessionStatisticsDto
{
    public int TotalActiveSessions { get; set; }

    public int InProgressSessions { get; set; }

    public int WaitingSessions { get; set; }

    public int CompletedSessions { get; set; }

    public int PotentiallyInactiveSessions { get; set; }

    public int OrphanedGameUsers { get; set; }

    public int OrphanedGameQuestions { get; set; }

    public DateTime? OldestActiveSession { get; set; }

    public DateTime? MostRecentActivity { get; set; }

    public int SessionTimeoutHours { get; set; }

    public DateTime GeneratedAt { get; set; }

    public int TotalCleanupCandidates => PotentiallyInactiveSessions + OrphanedGameUsers + OrphanedGameQuestions;
}