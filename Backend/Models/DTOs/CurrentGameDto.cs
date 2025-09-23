namespace Backend.Models.DTOs;

public class CurrentGameDto
{
    public long CurrentGameId { get; set; }
    public long QuizId { get; set; }
    public required string SessionId { get; set; }
    public bool IsStarted { get; set; }
    public bool IsCompleted { get; set; }
    public List<CurrentGameQuestionDto> CurrentGameQuestions { get; set; } = [];
    public List<CurrentGameUserDto> CurrentGameUsers { get; set; } = [];
}