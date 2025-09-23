namespace Backend.Models.Domains;

public class QuestionGroupQuiz : AuditableEntity
{
    public long QuestionGroupId { get; set; }
    public long QuizId { get; set; }
}