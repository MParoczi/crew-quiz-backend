namespace Backend.Models.Domains;

public class QuestionGroup : AuditableEntity
{
    public long QuestionGroupId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public List<Question> Questions { get; } = [];
    public List<Quiz> Quizzes { get; } = [];
}