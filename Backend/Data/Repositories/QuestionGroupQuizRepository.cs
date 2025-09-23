using Backend.Interfaces.Data.Repositories;
using Backend.Models.Domains;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class QuestionGroupQuizRepository(CrewQuizContext context, IHttpContextAccessor httpContextAccessor)
    : GenericRepository<QuestionGroupQuiz>(context, httpContextAccessor), IQuestionGroupQuizRepository
{
    private readonly CrewQuizContext _context = context;

    public async Task<IEnumerable<QuestionGroupQuiz>> GetByQuizId(long quizId)
    {
        var userId = GetCurrentUserId();

        return await _context.QuestionGroupQuiz
            .Where(qgq => qgq.QuizId == quizId)
            .Where(qgq => qgq.CreatedByUserId == userId)
            .OrderBy(qgq => qgq.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<QuestionGroupQuiz>> GetByQuestionGroupId(long questionGroupId)
    {
        var userId = GetCurrentUserId();

        return await _context.QuestionGroupQuiz
            .Where(qgq => qgq.QuestionGroupId == questionGroupId)
            .Where(qgq => qgq.CreatedByUserId == userId)
            .OrderBy(qgq => qgq.CreatedOn)
            .ToListAsync();
    }

    public async Task ClearQuestionGroupsFromQuiz(long? quizId)
    {
        var userId = GetCurrentUserId();

        var questionGroupQuizzes = await _context.QuestionGroupQuiz
            .Where(qgq => qgq.QuizId == quizId && qgq.CreatedByUserId == userId)
            .ToListAsync();

        _context.QuestionGroupQuiz.RemoveRange(questionGroupQuizzes);
    }

    public override Task<bool> UpdateAsync(QuestionGroupQuiz entity)
    {
        return Task.FromResult(true);
    }

    public override async Task<bool> RemoveAsync(QuestionGroupQuiz entity)
    {
        var rowsAffected = await _context.QuestionGroupQuiz
            .Where(qgq => qgq.QuestionGroupId == entity.QuestionGroupId && qgq.QuizId == entity.QuizId)
            .ExecuteDeleteAsync();

        return rowsAffected > 0;
    }
}