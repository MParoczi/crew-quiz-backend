namespace Backend.Models.Domains;

public class PreviousGame : AuditableEntity
{
    public long PreviousGameId { get; set; }
    public required string SessionId { get; set; }
    public required string QuizName { get; set; }
    public DateTime CompletedOn { get; set; }

    public List<PreviousGameUser> PreviousGameUsers { get; set; } = [];
}