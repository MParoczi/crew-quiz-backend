using Backend.Interfaces.Data.Repositories;
using Backend.Models.Domains;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class QuizRepository(CrewQuizContext context, IHttpContextAccessor httpContextAccessor)
    : GenericRepository<Quiz>(context, httpContextAccessor), IQuizRepository
{
    private readonly CrewQuizContext _context = context;

    public async Task<IEnumerable<Quiz>> GetAllQuizzesByUserId(long userId)
    {
        return await _context.User
            .Include(u => u.Quizzes.OrderBy(q => q.Name))
            .ThenInclude(q => q.QuestionGroups)
            .Where(u => u.UserId == userId)
            .SelectMany(u => u.Quizzes)
            .ToListAsync();
    }

    public async Task<Quiz?> GetQuizByCurrentGameId(long currentGameId)
    {
        var userId = GetCurrentUserId();

        return await _context.CurrentGame
            .Include(g => g.Quiz)
            .Where(g => g.CurrentGameId == currentGameId)
            .Select(g => g.Quiz)
            .Where(q => q.CreatedByUserId == userId)
            .FirstOrDefaultAsync();
    }

    public override async Task<bool> UpdateAsync(Quiz entity)
    {
        var rowsAffected = await context.Quiz
            .Where(q => q.QuizId == entity.QuizId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(q => q.Name, entity.Name));

        return rowsAffected > 0;
    }

    public override async Task<bool> RemoveAsync(Quiz entity)
    {
        var rowsAffected = await context.Quiz
            .Where(q => q.QuizId == entity.QuizId)
            .ExecuteDeleteAsync();

        return rowsAffected > 0;
    }
}