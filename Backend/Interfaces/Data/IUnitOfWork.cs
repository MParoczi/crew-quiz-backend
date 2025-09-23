using Backend.Interfaces.Data.Repositories;

namespace Backend.Interfaces.Data;

public interface IUnitOfWork : IDisposable
{
    public IUserRepository Users { get; }
    public IQuestionRepository Questions { get; }
    public IQuestionGroupRepository QuestionGroups { get; }
    public IQuizRepository Quizzes { get; }
    public ICurrentGameRepository CurrentGames { get; }
    public ICurrentGameUserRepository CurrentGameUsers { get; }
    public ICurrentGameQuestionRepository CurrentGameQuestions { get; }
    public IQuestionGroupQuizRepository QuestionGroupQuizzes { get; }
    public IPreviousGameRepository PreviousGames { get; }
    public IPreviousGameUserRepository PreviousGameUsers { get; }
    public int Complete();
    public Task<int> CompleteAsync();
}