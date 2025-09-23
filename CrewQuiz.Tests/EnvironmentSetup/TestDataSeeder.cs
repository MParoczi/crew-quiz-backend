using System.Security.Cryptography;
using Backend.Constants;
using Backend.Data;
using Backend.Models.Domains;
using Microsoft.EntityFrameworkCore;

namespace CrewQuiz.Tests.EnvironmentSetup;

/// <summary>
///     Provides consistent test data seeding functionality for ENV-2 testing
///     Ensures predictable baseline data for database seeding and migration tests
/// </summary>
public class TestDataSeeder
{
    private static readonly DateTime _baseDateTime = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private readonly CrewQuizContext _context;

    public TestDataSeeder(CrewQuizContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Seeds the database with consistent baseline data for testing
    /// </summary>
    public async Task SeedTestDataAsync()
    {
        // Clear existing data to ensure clean state
        await ClearAllDataAsync();

        // Seed Users first (required for audit fields)
        var users = await SeedUsersAsync();

        // Seed Quizzes
        var quizzes = await SeedQuizzesAsync(users[0]);

        // Seed Question Groups
        var questionGroups = await SeedQuestionGroupsAsync(users[0]);

        // Seed Questions
        var questions = await SeedQuestionsAsync(users[0], questionGroups[0]);

        // Seed Question Group Quiz relationships
        await SeedQuestionGroupQuizzesAsync(users[0], questionGroups, quizzes);

        // Seed Current Games
        var currentGames = await SeedCurrentGamesAsync(users[0]);

        // Seed Current Game Users
        await SeedCurrentGameUsersAsync(users, currentGames[0]);

        // Seed Current Game Questions
        await SeedCurrentGameQuestionsAsync(users[0], currentGames[0], questions);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    ///     Clears all data from the database to ensure clean test state
    /// </summary>
    public async Task ClearAllDataAsync()
    {
        // Clear change tracker to avoid entity tracking conflicts
        _context.ChangeTracker.Clear();

        // Load entities and remove in dependency order to avoid foreign key constraints
        var currentGameQuestions = await _context.CurrentGameQuestion.ToListAsync();
        var currentGameUsers = await _context.CurrentGameUser.ToListAsync();
        var currentGames = await _context.CurrentGame.ToListAsync();
        var questionGroupQuizzes = await _context.QuestionGroupQuiz.ToListAsync();
        var questions = await _context.Question.ToListAsync();
        var questionGroups = await _context.QuestionGroup.ToListAsync();
        var quizzes = await _context.Quiz.ToListAsync();
        var users = await _context.User.ToListAsync();

        // Remove in dependency order
        if (currentGameQuestions.Any()) _context.CurrentGameQuestion.RemoveRange(currentGameQuestions);

        if (currentGameUsers.Any()) _context.CurrentGameUser.RemoveRange(currentGameUsers);

        if (currentGames.Any()) _context.CurrentGame.RemoveRange(currentGames);

        if (questionGroupQuizzes.Any()) _context.QuestionGroupQuiz.RemoveRange(questionGroupQuizzes);

        if (questions.Any()) _context.Question.RemoveRange(questions);

        if (questionGroups.Any()) _context.QuestionGroup.RemoveRange(questionGroups);

        if (quizzes.Any()) _context.Quiz.RemoveRange(quizzes);

        if (users.Any()) _context.User.RemoveRange(users);

        await _context.SaveChangesAsync();

        // Clear change tracker again after save
        _context.ChangeTracker.Clear();
    }

    /// <summary>
    ///     Seeds test users with consistent data
    /// </summary>
    private async Task<List<User>> SeedUsersAsync()
    {
        // Generate proper password hash for test password "testpassword"
        // Using the same format as the authentication system: hash-salt in hex
        var testPasswordHash = GeneratePasswordHash("testpassword");

        var users = new List<User>
        {
            new()
            {
                UserId = 1,
                FirstName = "Test",
                LastName = "User",
                Username = "testuser1",
                PasswordHash = testPasswordHash,
                CreatedOn = _baseDateTime,
                CreatedByUserId = null
            },
            new()
            {
                UserId = 2,
                FirstName = "Admin",
                LastName = "User",
                Username = "adminuser",
                PasswordHash = testPasswordHash,
                CreatedOn = _baseDateTime.AddMinutes(1),
                CreatedByUserId = 1
            },
            new()
            {
                UserId = 3,
                FirstName = "Player",
                LastName = "One",
                Username = "player1",
                PasswordHash = testPasswordHash,
                CreatedOn = _baseDateTime.AddMinutes(2),
                CreatedByUserId = 1
            }
        };

        await _context.User.AddRangeAsync(users);
        return users;
    }

    /// <summary>
    ///     Seeds test quizzes with consistent data
    /// </summary>
    private async Task<List<Quiz>> SeedQuizzesAsync(User createdByUser)
    {
        var quizzes = new List<Quiz>
        {
            new()
            {
                QuizId = 1,
                Name = "Test Quiz 1",
                CreatedOn = _baseDateTime.AddMinutes(5),
                CreatedByUserId = createdByUser.UserId
            },
            new()
            {
                QuizId = 2,
                Name = "Test Quiz 2",
                CreatedOn = _baseDateTime.AddMinutes(10),
                CreatedByUserId = createdByUser.UserId
            }
        };

        await _context.Quiz.AddRangeAsync(quizzes);
        return quizzes;
    }

    /// <summary>
    ///     Seeds test question groups with consistent data
    /// </summary>
    private async Task<List<QuestionGroup>> SeedQuestionGroupsAsync(User createdByUser)
    {
        var questionGroups = new List<QuestionGroup>
        {
            new()
            {
                QuestionGroupId = 1,
                Name = "Test Question Group 1",
                Description = "A test question group for ENV-2 testing",
                CreatedOn = _baseDateTime.AddMinutes(3),
                CreatedByUserId = createdByUser.UserId
            },
            new()
            {
                QuestionGroupId = 2,
                Name = "Test Question Group 2",
                Description = "Another test question group for ENV-2 testing",
                CreatedOn = _baseDateTime.AddMinutes(4),
                CreatedByUserId = createdByUser.UserId
            }
        };

        await _context.QuestionGroup.AddRangeAsync(questionGroups);
        return questionGroups;
    }

    /// <summary>
    ///     Seeds test questions with consistent data
    /// </summary>
    private async Task<List<Question>> SeedQuestionsAsync(User createdByUser, QuestionGroup questionGroup)
    {
        var questions = new List<Question>
        {
            new()
            {
                QuestionId = 1,
                Inquiry = "What is the capital of France?",
                Answer = "Paris",
                Point = 10,
                QuestionGroupId = questionGroup.QuestionGroupId,
                CreatedOn = _baseDateTime.AddMinutes(6),
                CreatedByUserId = createdByUser.UserId
            },
            new()
            {
                QuestionId = 2,
                Inquiry = "What is 2 + 2?",
                Answer = "4",
                Point = 5,
                QuestionGroupId = questionGroup.QuestionGroupId,
                CreatedOn = _baseDateTime.AddMinutes(7),
                CreatedByUserId = createdByUser.UserId
            }
        };

        await _context.Question.AddRangeAsync(questions);
        return questions;
    }

    /// <summary>
    ///     Seeds question group quiz relationships
    /// </summary>
    private async Task SeedQuestionGroupQuizzesAsync(User createdByUser, List<QuestionGroup> questionGroups, List<Quiz> quizzes)
    {
        var questionGroupQuizzes = new List<QuestionGroupQuiz>
        {
            new()
            {
                QuestionGroupId = questionGroups[0].QuestionGroupId,
                QuizId = quizzes[0].QuizId,
                CreatedOn = _baseDateTime.AddMinutes(8),
                CreatedByUserId = createdByUser.UserId
            }
        };

        await _context.QuestionGroupQuiz.AddRangeAsync(questionGroupQuizzes);
    }

    /// <summary>
    ///     Seeds current games for testing
    /// </summary>
    private async Task<List<CurrentGame>> SeedCurrentGamesAsync(User createdByUser)
    {
        var currentGames = new List<CurrentGame>
        {
            new()
            {
                CurrentGameId = 1,
                SessionId = "test-session-123",
                QuizId = 1,
                CreatedOn = _baseDateTime.AddMinutes(15),
                CreatedByUserId = createdByUser.UserId
            }
        };

        await _context.CurrentGame.AddRangeAsync(currentGames);
        return currentGames;
    }

    /// <summary>
    ///     Seeds current game users for testing
    /// </summary>
    private async Task SeedCurrentGameUsersAsync(List<User> users, CurrentGame currentGame)
    {
        var currentGameUsers = new List<CurrentGameUser>
        {
            new()
            {
                CurrentGameId = currentGame.CurrentGameId,
                UserId = users[0].UserId,
                Points = 0,
                IsCurrent = true,
                IsGameMaster = true,
                CreatedOn = _baseDateTime.AddMinutes(16),
                CreatedByUserId = users[0].UserId
            },
            new()
            {
                CurrentGameId = currentGame.CurrentGameId,
                UserId = users[2].UserId,
                Points = 0,
                IsCurrent = true,
                IsGameMaster = false,
                CreatedOn = _baseDateTime.AddMinutes(17),
                CreatedByUserId = users[0].UserId
            }
        };

        await _context.CurrentGameUser.AddRangeAsync(currentGameUsers);
    }

    /// <summary>
    ///     Seeds current game questions for testing
    /// </summary>
    private async Task SeedCurrentGameQuestionsAsync(User createdByUser, CurrentGame currentGame, List<Question> questions)
    {
        var currentGameQuestions = new List<CurrentGameQuestion>
        {
            new()
            {
                CurrentGameId = currentGame.CurrentGameId,
                QuestionId = questions[0].QuestionId,
                IsAnswered = false,
                IsCurrent = true,
                IsRobbingAllowed = true,
                CreatedOn = _baseDateTime.AddMinutes(18),
                CreatedByUserId = createdByUser.UserId
            },
            new()
            {
                CurrentGameId = currentGame.CurrentGameId,
                QuestionId = questions[1].QuestionId,
                IsAnswered = false,
                IsCurrent = false,
                IsRobbingAllowed = false,
                CreatedOn = _baseDateTime.AddMinutes(19),
                CreatedByUserId = createdByUser.UserId
            }
        };

        await _context.CurrentGameQuestion.AddRangeAsync(currentGameQuestions);
    }

    /// <summary>
    ///     Gets count of all entities for verification
    /// </summary>
    public async Task<Dictionary<string, int>> GetEntityCountsAsync()
    {
        return new Dictionary<string, int>
        {
            ["Users"] = await _context.User.CountAsync(),
            ["Quizzes"] = await _context.Quiz.CountAsync(),
            ["QuestionGroups"] = await _context.QuestionGroup.CountAsync(),
            ["Questions"] = await _context.Question.CountAsync(),
            ["QuestionGroupQuizzes"] = await _context.QuestionGroupQuiz.CountAsync(),
            ["CurrentGames"] = await _context.CurrentGame.CountAsync(),
            ["CurrentGameUsers"] = await _context.CurrentGameUser.CountAsync(),
            ["CurrentGameQuestions"] = await _context.CurrentGameQuestion.CountAsync()
        };
    }

    /// <summary>
    ///     Generates a password hash in the format expected by the authentication system (hash-salt in hex)
    /// </summary>
    private string GeneratePasswordHash(string password)
    {
        // Generate a random salt
        var salt = new byte[32];
        RandomNumberGenerator.Fill(salt);

        // Generate hash using the same parameters as the authentication system
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Cryptography.Iterations, Cryptography.Algorithm, Cryptography.HashSize);

        // Return in the format expected: hash-salt (both as hex strings)
        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }
}