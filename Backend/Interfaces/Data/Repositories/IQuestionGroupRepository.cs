using Backend.Models.Domains;

namespace Backend.Interfaces.Data.Repositories;

public interface IQuestionGroupRepository : IGenericRepository<QuestionGroup>
{
    public Task<IEnumerable<QuestionGroup>> GetQuestionGroupsByUserId(long userId);
    public Task<IEnumerable<QuestionGroup>> GetQuestionGroupsByQuizId(long quizId);
}