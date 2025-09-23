using Backend.Interfaces.Data.Repositories;
using Backend.Models.Domains;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class CurrentGameUserRepository(CrewQuizContext context, IHttpContextAccessor httpContextAccessor)
    : GenericRepository<CurrentGameUser>(context, httpContextAccessor), ICurrentGameUserRepository
{
    private readonly CrewQuizContext _context = context;

    public override async Task<bool> UpdateAsync(CurrentGameUser entity)
    {
        var rowsAffected = await _context.CurrentGameUser
            .Where(cgu => cgu.UserId == entity.UserId && cgu.CurrentGameId == entity.CurrentGameId)
            .ExecuteUpdateAsync(cgu => cgu
                .SetProperty(cgu => cgu.IsCurrent, entity.IsCurrent)
                .SetProperty(cgu => cgu.Points, entity.Points));

        return rowsAffected > 0;
    }

    public override async Task<bool> RemoveAsync(CurrentGameUser entity)
    {
        var rowsAffected = await _context.CurrentGameUser
            .Where(cgu => cgu.UserId == entity.UserId && cgu.CurrentGameId == entity.CurrentGameId)
            .ExecuteDeleteAsync();

        return rowsAffected > 0;
    }
}