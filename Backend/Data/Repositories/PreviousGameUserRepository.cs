using Backend.Interfaces.Data.Repositories;
using Backend.Models.Domains;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class PreviousGameUserRepository(CrewQuizContext context, IHttpContextAccessor httpContextAccessor)
    : GenericRepository<PreviousGameUser>(context, httpContextAccessor), IPreviousGameUserRepository
{
    private readonly CrewQuizContext _context = context;

    public override async Task<bool> UpdateAsync(PreviousGameUser entity)
    {
        var rowsAffected = await _context.PreviousGameUser
            .Where(pgu => pgu.PreviousGameId == entity.PreviousGameId && pgu.UserId == entity.UserId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(pgu => pgu.Username, entity.Username)
                .SetProperty(pgu => pgu.IsGameMaster, entity.IsGameMaster)
                .SetProperty(pgu => pgu.Points, entity.Points)
                .SetProperty(pgu => pgu.Rank, entity.Rank));

        return rowsAffected > 0;
    }

    public override async Task<bool> RemoveAsync(PreviousGameUser entity)
    {
        var rowsAffected = await _context.PreviousGameUser
            .Where(pgu => pgu.PreviousGameId == entity.PreviousGameId && pgu.UserId == entity.UserId)
            .ExecuteDeleteAsync();

        return rowsAffected > 0;
    }
}