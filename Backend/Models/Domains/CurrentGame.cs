namespace Backend.Models.Domains;

public class CurrentGame : AuditableEntity
{
    public long CurrentGameId { get; set; }
    public required string SessionId { get; set; }
    public required long QuizId { get; set; }
    public bool IsStarted { get; set; }
    public bool IsCompleted { get; set; }

    public Quiz Quiz { get; set; }

    public List<CurrentGameQuestion> CurrentGameQuestions { get; set; } = [];
    public List<CurrentGameUser> CurrentGameUsers { get; set; } = [];
}