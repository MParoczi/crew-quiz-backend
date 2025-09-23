using System.Linq.Expressions;
using Backend.Interfaces.Data.Repositories;
using Backend.Models.Domains;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class PreviousGameRepository(CrewQuizContext context, IHttpContextAccessor httpContextAccessor)
    : GenericRepository<PreviousGame>(context, httpContextAccessor), IPreviousGameRepository
{
    private readonly CrewQuizContext _context = context;

    public async Task<PreviousGame?> GetPreviousGameWithIncludesAsync(Expression<Func<PreviousGame, bool>> expression)
    {
        var previousGame = await _context.PreviousGame
            .Include(cg => cg.PreviousGameUsers.OrderBy(cgu => cgu.Rank))
            .ThenInclude(cgu => cgu.User)
            .FirstOrDefaultAsync(expression);

        return previousGame;
    }

    public async Task<IEnumerable<PreviousGame>> GetPreviousGamesByUserIdAsync(long userId)
    {
        var previousGames = await _context.PreviousGame
            .Where(pg => pg.PreviousGameUsers.Any(pgu => pgu.UserId == userId))
            .Include(pg => pg.PreviousGameUsers.OrderBy(pgu => pgu.Rank))
            .OrderByDescending(pg => pg.CompletedOn)
            .ToListAsync();

        return previousGames;
    }

    public override async Task<bool> UpdateAsync(PreviousGame entity)
    {
        var rowsAffected = await _context.PreviousGame
            .Where(pg => pg.PreviousGameId == entity.PreviousGameId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(pg => pg.QuizName, entity.QuizName)
                .SetProperty(pg => pg.CompletedOn, entity.CompletedOn));

        return rowsAffected > 0;
    }

    public override async Task<bool> RemoveAsync(PreviousGame entity)
    {
        var rowsAffected = await _context.PreviousGame.Where(pg => pg.PreviousGameId == entity.PreviousGameId).ExecuteDeleteAsync();

        return rowsAffected > 0;
    }
}