using Backend.Models.DTOs;

namespace Backend.Interfaces.Services;

public interface IQuestionGroupService : IServiceBase
{
    public Task<IEnumerable<QuestionGroupDto>> GetQuestionGroupsForCurrentUser();
    public Task<IEnumerable<QuestionGroupDto>> GetQuestionGroupsByQuizId(long quizId);
    public Task<QuestionGroupDto> GetQuestionGroup(long questionGroupId);
    public Task CreateQuestionGroup(QuestionGroupDto questionGroup);
    public Task UpdateQuestionGroup(QuestionGroupDto questionGroup);
    public Task DeleteQuestionGroup(long questionGroupId);
}