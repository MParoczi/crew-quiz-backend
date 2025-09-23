using Backend.Models.DTOs;

namespace Backend.Interfaces.Services;

public interface IQuestionService : IServiceBase
{
    public Task<IEnumerable<QuestionDto>> GetQuestionsForCurrentUser();
    public Task<IEnumerable<QuestionDto>> GetQuestionsByQuestionGroupId(long questionGroupId);
    public Task<QuestionDto> GetQuestion(long questionId);
    public Task CreateQuestion(QuestionDto questionDto);
    public Task UpdateQuestion(QuestionDto questionDto);
    public Task DeleteQuestion(long questionId);
}