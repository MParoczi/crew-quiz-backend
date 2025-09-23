using Backend.Interfaces.Data;
using Backend.Interfaces.Data.Repositories;

namespace Backend.Data;

public class UnitOfWork(
    CrewQuizContext context,
    IUserRepository userRepository,
    IQuestionRepository questionRepository,
    IQuestionGroupRepository questionGroupRepository,
    IQuizRepository quizRepository,
    ICurrentGameRepository currentGameRepository,
    ICurrentGameUserRepository currentGameUserRepository,
    ICurrentGameQuestionRepository currentGameQuestionRepository,
    IQuestionGroupQuizRepository questionGroupQuizRepository,
    IPreviousGameRepository previousGameRepository,
    IPreviousGameUserRepository previousGameUserRepository) : IUnitOfWork
{
    public IQuestionGroupRepository QuestionGroups { get; } = questionGroupRepository;
    public IUserRepository Users { get; } = userRepository;
    public IQuestionRepository Questions { get; } = questionRepository;
    public IQuizRepository Quizzes { get; } = quizRepository;
    public ICurrentGameRepository CurrentGames { get; } = currentGameRepository;
    public ICurrentGameQuestionRepository CurrentGameQuestions { get; } = currentGameQuestionRepository;
    public ICurrentGameUserRepository CurrentGameUsers { get; } = currentGameUserRepository;
    public IQuestionGroupQuizRepository QuestionGroupQuizzes { get; } = questionGroupQuizRepository;
    public IPreviousGameRepository PreviousGames { get; } = previousGameRepository;
    public IPreviousGameUserRepository PreviousGameUsers { get; } = previousGameUserRepository;

    public int Complete()
    {
        return context.SaveChanges();
    }

    public async Task<int> CompleteAsync()
    {
        return await context.SaveChangesAsync();
    }

    public void Dispose()
    {
        context.Dispose();
    }
}