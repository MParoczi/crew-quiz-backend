namespace Backend.Models.Configurations;

public class AppSettings
{
    public required string Environment { get; init; }
    public required Jwt Jwt { get; init; }
    public required Cors Cors { get; init; }
    public required SessionCleanup SessionCleanup { get; init; }
}