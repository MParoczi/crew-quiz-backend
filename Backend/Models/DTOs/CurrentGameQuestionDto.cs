namespace Backend.Models.DTOs;

public class CurrentGameQuestionDto
{
    public bool IsAnswered { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsRobbingAllowed { get; set; }
    public string? AnswerHint { get; set; }
    public required QuestionDto Question { get; set; }
}