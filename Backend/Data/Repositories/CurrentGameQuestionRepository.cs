using Backend.Interfaces.Data.Repositories;
using Backend.Models.Domains;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class CurrentGameQuestionRepository(CrewQuizContext context, IHttpContextAccessor httpContextAccessor)
    : GenericRepository<CurrentGameQuestion>(context, httpContextAccessor), ICurrentGameQuestionRepository
{
    private readonly CrewQuizContext _context = context;

    public override async Task<bool> UpdateAsync(CurrentGameQuestion entity)
    {
        var rowsAffected = await _context.CurrentGameQuestion
            .Where(cgq => cgq.QuestionId == entity.QuestionId && cgq.CurrentGameId == entity.CurrentGameId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(cgq => cgq.IsAnswered, entity.IsAnswered)
                .SetProperty(cgq => cgq.IsCurrent, entity.IsCurrent)
                .SetProperty(cgq => cgq.IsRobbingAllowed, entity.IsRobbingAllowed)
                .SetProperty(cgq => cgq.AnsweredByUserId, entity.AnsweredByUserId));

        return rowsAffected > 0;
    }

    public override async Task<bool> RemoveAsync(CurrentGameQuestion entity)
    {
        var rowsAffected = await _context.CurrentGameQuestion
            .Where(cgq => cgq.QuestionId == entity.QuestionId && cgq.CurrentGameId == entity.CurrentGameId)
            .ExecuteDeleteAsync();

        return rowsAffected > 0;
    }
}