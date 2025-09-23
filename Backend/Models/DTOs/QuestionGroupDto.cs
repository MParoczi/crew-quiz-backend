namespace Backend.Models.DTOs;

public class QuestionGroupDto
{
    public long QuestionGroupId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public List<QuestionDto> Questions { get; set; } = [];
}