using Backend.Models.Domains;

namespace Backend.Interfaces.Data.Repositories;

public interface IQuestionRepository : IGenericRepository<Question>
{
    public Task<IEnumerable<Question>> GetQuestionsByUserId(long userId);
    public Task<IEnumerable<Question>> GetQuestionsByQuestionGroupId(long questionGroupId);
    public Task<IEnumerable<Question>> GetQuestionsByQuizId(long quizId);
}