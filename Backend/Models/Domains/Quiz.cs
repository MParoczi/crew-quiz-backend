namespace Backend.Models.Domains;

public class Quiz : AuditableEntity
{
    public long QuizId { get; set; }
    public required string Name { get; set; }

    public List<QuestionGroup> QuestionGroups { get; } = [];
    public List<CurrentGame> CurrentGames { get; } = [];
}