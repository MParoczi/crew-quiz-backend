using Backend.Models.DTOs;

namespace Backend.Interfaces.Services;

public interface IQuizService : IServiceBase
{
    public Task<IEnumerable<QuizDto>> GetQuizzesForCurrentUser();
    public Task<QuizDto> GetQuizByCurrentGameId(long currentGameId);
    public Task<QuizDto> GetQuiz(long quizId);
    public Task CreateQuiz(QuizDto quizDto);
    public Task UpdateQuiz(QuizDto quizDto);
    public Task DeleteQuiz(long quizId);
}