using Backend.Models.Domains;

namespace Backend.Interfaces.Data.Repositories;

public interface IQuestionGroupQuizRepository : IGenericRepository<QuestionGroupQuiz>
{
    Task<IEnumerable<QuestionGroupQuiz>> GetByQuizId(long quizId);
    Task<IEnumerable<QuestionGroupQuiz>> GetByQuestionGroupId(long questionGroupId);
    Task ClearQuestionGroupsFromQuiz(long? quizId);
}