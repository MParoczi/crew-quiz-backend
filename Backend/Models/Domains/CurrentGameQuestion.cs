namespace Backend.Models.Domains;

public class CurrentGameQuestion : AuditableEntity
{
    public required long CurrentGameId { get; set; }
    public required long QuestionId { get; set; }
    public bool IsAnswered { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsRobbingAllowed { get; set; }
    public long? AnsweredByUserId { get; set; }

    public CurrentGame CurrentGame { get; set; }
    public Question Question { get; set; }
    public User? AnsweredByUser { get; set; }
}