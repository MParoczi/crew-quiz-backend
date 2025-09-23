using System.Linq.Expressions;
using Backend.Interfaces.Data.Repositories;
using Backend.Models.Domains;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class CurrentGameRepository(CrewQuizContext context, IHttpContextAccessor httpContextAccessor)
    : GenericRepository<CurrentGame>(context, httpContextAccessor), ICurrentGameRepository
{
    private readonly CrewQuizContext _context = context;

    public async Task<CurrentGame?> GetCurrentGameWithIncludesAsync(Expression<Func<CurrentGame, bool>> expression)
    {
        var currentGame = await _context.CurrentGame
            .Include(cg => cg.Quiz)
            .Include(cg => cg.CurrentGameQuestions.OrderBy(cgq => cgq.CreatedOn))
            .ThenInclude(cgq => cgq.Question)
            .ThenInclude(q => q.QuestionGroup)
            .Include(cg => cg.CurrentGameUsers.OrderBy(cgu => cgu.CreatedOn))
            .ThenInclude(cgu => cgu.User)
            .FirstOrDefaultAsync(expression);

        return currentGame;
    }

    public override async Task<bool> UpdateAsync(CurrentGame entity)
    {
        var rowsAffected = await _context.CurrentGame
            .Where(cg => cg.CurrentGameId == entity.CurrentGameId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(cg => cg.IsStarted, entity.IsStarted)
                .SetProperty(cg => cg.IsCompleted, entity.IsCompleted));

        return rowsAffected > 0;
    }

    public override async Task<bool> RemoveAsync(CurrentGame entity)
    {
        // Perform cascade deletion
        await _context.CurrentGameQuestion
            .Where(cgq => cgq.CurrentGameId == entity.CurrentGameId)
            .ExecuteDeleteAsync();

        await _context.CurrentGameUser
            .Where(cgu => cgu.CurrentGameId == entity.CurrentGameId)
            .ExecuteDeleteAsync();

        var rowsAffected = await _context.CurrentGame
            .Where(cg => cg.CurrentGameId == entity.CurrentGameId)
            .ExecuteDeleteAsync();

        return rowsAffected > 0;
    }
}