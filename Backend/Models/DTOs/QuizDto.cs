namespace Backend.Models.DTOs;

public class QuizDto
{
    public long QuizId { get; set; }
    public required string Name { get; set; }
    public List<QuestionGroupDto> QuestionGroups { get; set; } = [];
}