using Backend.Interfaces.Data.Repositories;
using Backend.Models.Domains;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class QuestionGroupRepository(CrewQuizContext context, IHttpContextAccessor httpContextAccessor)
    : GenericRepository<QuestionGroup>(context, httpContextAccessor), IQuestionGroupRepository
{
    private readonly CrewQuizContext _context = context;

    public async Task<IEnumerable<QuestionGroup>> GetQuestionGroupsByUserId(long userId)
    {
        return await _context.User
            .Include(u => u.QuestionGroups.OrderBy(qg => qg.Name))
            .ThenInclude(qg => qg.Questions.OrderBy(q => q.Point))
            .Where(u => u.UserId == userId)
            .SelectMany(u => u.QuestionGroups)
            .ToListAsync();
    }

    public async Task<IEnumerable<QuestionGroup>> GetQuestionGroupsByQuizId(long quizId)
    {
        var userId = GetCurrentUserId();

        return await _context.Quiz
            .Include(q => q.QuestionGroups)
            .ThenInclude(qg => qg.Questions)
            .Where(q => q.QuizId == quizId)
            .SelectMany(q => q.QuestionGroups)
            .Where(qg => qg.CreatedByUserId == userId)
            .ToListAsync();
    }

    public override async Task<bool> UpdateAsync(QuestionGroup entity)
    {
        var rowsAffected = await _context.QuestionGroup
            .Where(qg => qg.QuestionGroupId == entity.QuestionGroupId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(qg => qg.Name, entity.Name)
                .SetProperty(qg => qg.Description, entity.Description));

        return rowsAffected > 0;
    }

    public override async Task<bool> RemoveAsync(QuestionGroup entity)
    {
        var questionRowsAffected = await _context.Question.Where(q => q.QuestionGroupId == entity.QuestionGroupId).ExecuteDeleteAsync();

        var rowsAffected = await _context.QuestionGroup
            .Where(qg => qg.QuestionGroupId == entity.QuestionGroupId)
            .ExecuteDeleteAsync();

        return rowsAffected > 0;
    }
}