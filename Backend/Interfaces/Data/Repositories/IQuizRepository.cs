using Backend.Models.Domains;

namespace Backend.Interfaces.Data.Repositories;

public interface IQuizRepository : IGenericRepository<Quiz>
{
    public Task<IEnumerable<Quiz>> GetAllQuizzesByUserId(long userId);
    public Task<Quiz?> GetQuizByCurrentGameId(long currentGameId);
}