using Backend.Interfaces.Data.Repositories;
using Backend.Models.Domains;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class QuestionRepository(CrewQuizContext context, IHttpContextAccessor httpContextAccessor)
    : GenericRepository<Question>(context, httpContextAccessor), IQuestionRepository
{
    private readonly CrewQuizContext _context = context;

    public async Task<IEnumerable<Question>> GetQuestionsByUserId(long userId)
    {
        return await _context.User
            .Include(u => u.Questions.OrderBy(q => q.Point))
            .ThenInclude(q => q.QuestionGroup)
            .Where(u => u.UserId == userId)
            .SelectMany(u => u.Questions)
            .OrderBy(q => q.QuestionGroup.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Question>> GetQuestionsByQuestionGroupId(long questionGroupId)
    {
        var userId = GetCurrentUserId();

        return await _context.QuestionGroup
            .Include(qg => qg.Questions.OrderBy(q => q.Point))
            .ThenInclude(q => q.QuestionGroup)
            .Where(qg => qg.QuestionGroupId == questionGroupId)
            .SelectMany(qg => qg.Questions)
            .Where(q => q.CreatedByUserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Question>> GetQuestionsByQuizId(long quizId)
    {
        var userId = GetCurrentUserId();

        return await _context.Quiz
            .Include(qg => qg.QuestionGroups)
            .ThenInclude(qg => qg.Questions.OrderBy(q => q.Point))
            .ThenInclude(q => q.QuestionGroup)
            .Where(q => q.QuizId == quizId)
            .SelectMany(q => q.QuestionGroups)
            .SelectMany(qg => qg.Questions)
            .Where(q => q.CreatedByUserId == userId)
            .ToListAsync();
    }

    public override async Task<Question?> GetByIdAsync(object id)
    {
        return await _context.Question
            .Include(q => q.QuestionGroup)
            .FirstOrDefaultAsync(q => q.QuestionId == (long)id);
    }

    public override async Task<bool> UpdateAsync(Question entity)
    {
        var rowsAffected = await context.Question
            .Where(q => q.QuestionId == entity.QuestionId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(q => q.Inquiry, entity.Inquiry)
                .SetProperty(q => q.Answer, entity.Answer));

        return rowsAffected > 0;
    }

    public override async Task<bool> RemoveAsync(Question entity)
    {
        var rowsAffected = await context.Question.Where(q => q.QuestionId == entity.QuestionId).ExecuteDeleteAsync();

        return rowsAffected > 0;
    }
}