namespace Backend.Models.DTOs;

public class QuestionDto
{
    public long QuestionId { get; set; }
    public long? QuestionGroupId { get; set; }
    public string? QuestionGroupName { get; set; }
    public required string Inquiry { get; set; }
    public required string Answer { get; set; }
    public short Point { get; set; }
}