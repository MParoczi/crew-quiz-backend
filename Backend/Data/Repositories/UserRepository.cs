using Backend.Interfaces.Data.Repositories;
using Backend.Models.Domains;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class UserRepository(CrewQuizContext context, IHttpContextAccessor httpContextAccessor)
    : GenericRepository<User>(context, httpContextAccessor), IUserRepository
{
    private readonly CrewQuizContext _context = context;

    public override async Task<bool> UpdateAsync(User entity)
    {
        var existingUser = await _context.User.FirstOrDefaultAsync(u => u.UserId == entity.UserId);
        if (existingUser == null) return false;

        existingUser.FirstName = entity.FirstName;
        existingUser.LastName = entity.LastName;
        existingUser.Username = entity.Username;

        if (!string.IsNullOrEmpty(entity.PasswordHash)) existingUser.PasswordHash = entity.PasswordHash;

        var rowsAffected = await _context.SaveChangesAsync();
        return rowsAffected > 0;
    }

    public override async Task<bool> RemoveAsync(User entity)
    {
        var quizzes = await _context.Quiz.Where(q => q.CreatedByUserId == entity.UserId).ToListAsync();
        var questionGroups = await _context.QuestionGroup.Where(qg => qg.CreatedByUserId == entity.UserId).ToListAsync();
        var questions = await _context.Question.Where(q => q.CreatedByUserId == entity.UserId).ToListAsync();
        var currentGames = await _context.CurrentGame.Where(cg => cg.CreatedByUserId == entity.UserId).ToListAsync();
        var currentGameUsers = await _context.CurrentGameUser.Where(cgu => cgu.UserId == entity.UserId).ToListAsync();
        var currentGameQuestions = await _context.CurrentGameQuestion
            .Where(cgq => cgq.CreatedByUserId == entity.UserId || cgq.AnsweredByUserId == entity.UserId).ToListAsync();

        if (quizzes.Count > 0) _context.Quiz.RemoveRange(quizzes);

        if (questionGroups.Count > 0) _context.QuestionGroup.RemoveRange(questionGroups);

        if (questions.Count > 0) _context.Question.RemoveRange(questions);

        if (currentGames.Count > 0) _context.CurrentGame.RemoveRange(currentGames);

        if (currentGameUsers.Count > 0) _context.CurrentGameUser.RemoveRange(currentGameUsers);

        if (currentGameQuestions.Count > 0) _context.CurrentGameQuestion.RemoveRange(currentGameQuestions);

        _context.User.Remove(entity);

        var rowsAffected = await _context.SaveChangesAsync();
        return rowsAffected > 0;
    }
}