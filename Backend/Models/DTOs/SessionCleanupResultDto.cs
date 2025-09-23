namespace Backend.Models.DTOs;

public class SessionCleanupResultDto
{
    public int CleanedUpSessions { get; set; }

    public int CleanedUpGameUsers { get; set; }

    public int CleanedUpGameQuestions { get; set; }

    public TimeSpan ExecutionTime { get; set; }

    public DateTime CleanupTimestamp { get; set; }

    public int SessionTimeoutHours { get; set; }

    public List<string> Errors { get; set; } = [];

    public bool IsSuccess { get; set; }

    public int TotalRecordsAffected => CleanedUpSessions + CleanedUpGameUsers + CleanedUpGameQuestions;
}