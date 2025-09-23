namespace Backend.Models.Configurations;

public class SessionCleanup
{
    public required int IntervalHours { get; init; }
    public required int SessionTimeoutHours { get; init; }
}