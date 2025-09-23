namespace Backend.Models.Domains;

public class Question : AuditableEntity
{
    public long QuestionId { get; set; }
    public required string Inquiry { get; set; }
    public required string Answer { get; set; }
    public short Point { get; set; }
    public long QuestionGroupId { get; set; }

    public QuestionGroup QuestionGroup { get; set; }

    public List<CurrentGameQuestion> CurrentGameQuestions { get; set; } = [];
}