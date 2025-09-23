namespace Backend.Models.DTOs;

public class PreviousGameDto
{
    public long PreviousGameId { get; set; }
    public required string SessionId { get; set; }
    public required string QuizName { get; set; }
    public DateTime CompletedOn { get; set; }
    public List<PreviousGameUserDto> PreviousGameUsers { get; set; } = [];
}