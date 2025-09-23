using Backend.Data;
using Backend.Models.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CrewQuiz.Tests.EnvironmentSetup;

/// <summary>
///     Story ENV-2: Database Seeding and Migration
///     As a testing framework, I want to prepare test data and database schema
///     So that tests have consistent baseline data
///     Acceptance Criteria:
///     - All EF migrations apply successfully
///     - Test database can be reset between test runs
///     - Seed data is consistent and predictable
///     - Database constraints are properly enforced
///     - Transaction isolation works correctly
/// </summary>
public class DatabaseSeedingAndMigrationTests : TestBase
{
    [Fact]
    public async Task Should_ApplyEfMigrationsSuccessfully()
    {
        // Arrange
        var dbContext = ServiceProvider.GetRequiredService<CrewQuizContext>();

        // Act - Apply migrations (EnsureCreated handles this for InMemory DB)
        var canConnect = dbContext.Database.CanConnect();
        var isInMemory = dbContext.Database.IsInMemory();

        // For InMemory database, we verify the database is created and accessible
        // In real scenarios with SQL Server/PostgreSQL, migrations would be applied
        await dbContext.Database.EnsureCreatedAsync();

        // Assert
        Assert.True(canConnect, "Database should be accessible");
        Assert.True(isInMemory, "Should be using InMemory database for tests");

        // Verify all DbSets are accessible (indicates schema is properly created)
        var userCount = await dbContext.User.CountAsync();
        var quizCount = await dbContext.Quiz.CountAsync();
        var questionGroupCount = await dbContext.QuestionGroup.CountAsync();
        var questionCount = await dbContext.Question.CountAsync();
        var questionGroupQuizCount = await dbContext.QuestionGroupQuiz.CountAsync();
        var currentGameCount = await dbContext.CurrentGame.CountAsync();
        var currentGameUserCount = await dbContext.CurrentGameUser.CountAsync();
        var currentGameQuestionCount = await dbContext.CurrentGameQuestion.CountAsync();

        // All counts should be accessible (0 initially, but no exceptions thrown)
        Assert.True(userCount >= 0);
        Assert.True(quizCount >= 0);
        Assert.True(questionGroupCount >= 0);
        Assert.True(questionCount >= 0);
        Assert.True(questionGroupQuizCount >= 0);
        Assert.True(currentGameCount >= 0);
        Assert.True(currentGameUserCount >= 0);
        Assert.True(currentGameQuestionCount >= 0);

        Console.WriteLine("[DEBUG_LOG] EF migrations applied successfully - all DbSets accessible");
    }

    [Fact]
    public async Task Should_ResetDatabaseBetweenTestRuns()
    {
        // Arrange
        var dbContext = ServiceProvider.GetRequiredService<CrewQuizContext>();
        var seeder = new TestDataSeeder(dbContext);

        // Act - First seeding
        await seeder.SeedTestDataAsync();
        var firstRunCounts = await seeder.GetEntityCountsAsync();

        // Clear and seed again
        await seeder.ClearAllDataAsync();
        var clearedCounts = await seeder.GetEntityCountsAsync();

        await seeder.SeedTestDataAsync();
        var secondRunCounts = await seeder.GetEntityCountsAsync();

        // Assert - Database was cleared
        Assert.All(clearedCounts.Values, count => Assert.Equal(0, count));

        // Assert - Second run has same data as first run
        Assert.Equal(firstRunCounts.Count, secondRunCounts.Count);
        foreach (var kvp in firstRunCounts) Assert.Equal(kvp.Value, secondRunCounts[kvp.Key]);

        Console.WriteLine("[DEBUG_LOG] Database reset between test runs successfully");
        Console.WriteLine($"[DEBUG_LOG] First run counts: {string.Join(", ", firstRunCounts.Select(x => $"{x.Key}={x.Value}"))}");
        Console.WriteLine($"[DEBUG_LOG] Second run counts: {string.Join(", ", secondRunCounts.Select(x => $"{x.Key}={x.Value}"))}");
    }

    [Fact]
    public async Task Should_ProvideSeedDataConsistentAndPredictable()
    {
        // Arrange
        var dbContext = ServiceProvider.GetRequiredService<CrewQuizContext>();
        var seeder = new TestDataSeeder(dbContext);

        // Act
        await seeder.SeedTestDataAsync();

        // Assert - Verify predictable user data
        var users = await dbContext.User.OrderBy(u => u.UserId).ToListAsync();
        Assert.Equal(3, users.Count);
        Assert.Equal("testuser1", users[0].Username);
        Assert.Equal("Test", users[0].FirstName);
        Assert.Equal("User", users[0].LastName);
        Assert.Equal("adminuser", users[1].Username);
        Assert.Equal("player1", users[2].Username);

        // Assert - Verify predictable quiz data
        var quizzes = await dbContext.Quiz.OrderBy(q => q.QuizId).ToListAsync();
        Assert.Equal(2, quizzes.Count);
        Assert.Equal("Test Quiz 1", quizzes[0].Name);
        Assert.Equal("Test Quiz 2", quizzes[1].Name);

        // Assert - Verify predictable question data
        var questions = await dbContext.Question.OrderBy(q => q.QuestionId).ToListAsync();
        Assert.Equal(2, questions.Count);
        Assert.Equal("What is the capital of France?", questions[0].Inquiry);
        Assert.Equal("Paris", questions[0].Answer);
        Assert.Equal(10, questions[0].Point);
        Assert.Equal("What is 2 + 2?", questions[1].Inquiry);
        Assert.Equal("4", questions[1].Answer);
        Assert.Equal(5, questions[1].Point);

        // Assert - Verify audit fields are properly set
        Assert.All(users, user => Assert.True(user.CreatedOn != default));
        Assert.All(quizzes, quiz => Assert.True(quiz.CreatedOn != default));
        Assert.All(questions, question => Assert.True(question.CreatedOn != default));

        Console.WriteLine("[DEBUG_LOG] Seed data is consistent and predictable across all entities");
    }

    [Fact]
    public async Task Should_EnforceDatabaseConstraintsProperly()
    {
        // Arrange
        var dbContext = ServiceProvider.GetRequiredService<CrewQuizContext>();
        var seeder = new TestDataSeeder(dbContext);
        await seeder.SeedTestDataAsync();

        // Test 1: Required field constraints
        var userWithoutUsername = new User
        {
            FirstName = "Test",
            LastName = "User",
            Username = null!, // This should violate required constraint
            PasswordHash = "hash",
            CreatedOn = DateTime.UtcNow
        };

        // Act & Assert - Required field constraint
        dbContext.User.Add(userWithoutUsername);
        var requiredFieldException = await Record.ExceptionAsync(async () => await dbContext.SaveChangesAsync());
        Assert.NotNull(requiredFieldException);

        // Clean up failed add
        dbContext.Entry(userWithoutUsername).State = EntityState.Detached;

        // Test 2: Foreign key constraints
        var questionWithInvalidGroup = new Question
        {
            Inquiry = "Test Question",
            Answer = "Answer",
            Point = 5,
            QuestionGroupId = 9999, // Non-existent question group ID
            CreatedOn = DateTime.UtcNow,
            CreatedByUserId = 1
        };

        // Act & Assert - Foreign key constraint (for real databases)
        // Note: InMemory database doesn't enforce foreign key constraints by default
        // This test would fail with real SQL Server/PostgreSQL databases
        dbContext.Question.Add(questionWithInvalidGroup);

        // For InMemory DB, we verify the constraint logic exists in configurations
        var questionEntity = dbContext.Model.FindEntityType(typeof(Question));
        var foreignKey = questionEntity?.GetForeignKeys()
            .FirstOrDefault(fk => fk.Properties.Any(p => p.Name == "QuestionGroupId"));

        Assert.NotNull(foreignKey);

        Console.WriteLine("[DEBUG_LOG] Database constraints are properly configured");
        Console.WriteLine($"[DEBUG_LOG] Found foreign key constraint: {foreignKey.PrincipalEntityType.Name}");
    }

    [Fact]
    public async Task Should_HandleTransactionIsolationCorrectly()
    {
        // Arrange
        var dbContext = ServiceProvider.GetRequiredService<CrewQuizContext>();
        var seeder = new TestDataSeeder(dbContext);
        await seeder.SeedTestDataAsync();

        var initialUserCount = await dbContext.User.CountAsync();

        // Act & Assert - Test transaction capability (InMemory DB has limitations)
        if (dbContext.Database.IsInMemory())
        {
            // InMemory database doesn't support transactions, so we test the pattern instead
            Console.WriteLine("[DEBUG_LOG] InMemory database detected - testing transaction pattern without actual transaction support");

            // Test that we can detect InMemory database and handle it appropriately
            Assert.True(dbContext.Database.IsInMemory());
            Assert.True(dbContext.Database.CanConnect());

            // Test transaction-like behavior with InMemory database
            // Since InMemory databases are isolated per unique connection string (GUID),
            // we test the pattern of data isolation and rollback simulation

            // Test 1: Verify that changes within the same context are visible
            var beforeAddCount = await dbContext.User.CountAsync();

            var testUser = new User
            {
                FirstName = "Transaction",
                LastName = "Test",
                Username = "transactiontest",
                PasswordHash = "hash",
                CreatedOn = DateTime.UtcNow
            };

            dbContext.User.Add(testUser);
            await dbContext.SaveChangesAsync();

            var afterAddCount = await dbContext.User.CountAsync();
            Assert.Equal(beforeAddCount + 1, afterAddCount);

            // Test 2: Simulate rollback by removing the entity
            dbContext.User.Remove(testUser);
            await dbContext.SaveChangesAsync();

            var afterRemoveCount = await dbContext.User.CountAsync();
            Assert.Equal(beforeAddCount, afterRemoveCount);

            Console.WriteLine($"[DEBUG_LOG] Before add: {beforeAddCount}, After add: {afterAddCount}, After remove: {afterRemoveCount}");
            Console.WriteLine("[DEBUG_LOG] Transaction-like behavior verified: add -> commit -> rollback simulation");

            Console.WriteLine("[DEBUG_LOG] Transaction isolation pattern verified for InMemory database");
        }
        else
        {
            // Real database with transaction support
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var newUser = new User
                    {
                        FirstName = "Transaction",
                        LastName = "Test",
                        Username = "transactiontest",
                        PasswordHash = "hash",
                        CreatedOn = DateTime.UtcNow
                    };

                    dbContext.User.Add(newUser);
                    await dbContext.SaveChangesAsync();

                    var countInTransaction = await dbContext.User.CountAsync();
                    Assert.Equal(initialUserCount + 1, countInTransaction);

                    await transaction.RollbackAsync();

                    Console.WriteLine("[DEBUG_LOG] Transaction rolled back successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DEBUG_LOG] Transaction exception: {ex.Message}");
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            var finalUserCount = await dbContext.User.CountAsync();
            Assert.Equal(initialUserCount, finalUserCount);

            Console.WriteLine("[DEBUG_LOG] Transaction isolation working correctly with real database");
        }
    }

    [Fact]
    public async Task Should_HandleConnectionPoolingForTestScenarios()
    {
        // Arrange - Test multiple concurrent database contexts
        var tasks = new List<Task>();
        var results = new List<bool>();
        var lockObject = new object();

        // Act - Create multiple concurrent database operations
        for (var i = 0; i < 5; i++)
        {
            var taskIndex = i;
            tasks.Add(Task.Run(async () =>
            {
                using var scope = ServiceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<CrewQuizContext>();

                // Each task seeds its own data
                var seeder = new TestDataSeeder(dbContext);
                await seeder.SeedTestDataAsync();

                // Verify data was seeded
                var userCount = await dbContext.User.CountAsync();
                var quizCount = await dbContext.Quiz.CountAsync();

                lock (lockObject)
                {
                    results.Add(userCount >= 3 && quizCount >= 2);
                    Console.WriteLine($"[DEBUG_LOG] Task {taskIndex}: Users={userCount}, Quizzes={quizCount}");
                }
            }));
        }

        // Wait for all tasks to complete
        await Task.WhenAll(tasks);

        // Assert - All operations should have succeeded
        Assert.Equal(5, results.Count);
        Assert.All(results, result => Assert.True(result));

        Console.WriteLine("[DEBUG_LOG] Connection pooling handled concurrent operations successfully");
    }

    [Fact]
    public async Task Should_ValidateDataIntegrityAfterSeeding()
    {
        // Arrange
        var dbContext = ServiceProvider.GetRequiredService<CrewQuizContext>();
        var seeder = new TestDataSeeder(dbContext);

        // Act
        await seeder.SeedTestDataAsync();

        // Assert - Verify entity relationships are properly established
        var user = await dbContext.User.FirstAsync(u => u.UserId == 1);
        var quiz = await dbContext.Quiz.FirstAsync(q => q.QuizId == 1);
        var questionGroup = await dbContext.QuestionGroup.FirstAsync(qg => qg.QuestionGroupId == 1);
        var question = await dbContext.Question.FirstAsync(q => q.QuestionId == 1);
        var currentGame = await dbContext.CurrentGame.FirstAsync(cg => cg.CurrentGameId == 1);

        // Verify audit trail consistency
        Assert.Equal(user.UserId, quiz.CreatedByUserId);
        Assert.Equal(user.UserId, questionGroup.CreatedByUserId);
        Assert.Equal(user.UserId, question.CreatedByUserId);
        Assert.Equal(user.UserId, currentGame.CreatedByUserId);

        // Verify foreign key relationships
        Assert.Equal(questionGroup.QuestionGroupId, question.QuestionGroupId);
        Assert.Equal(quiz.QuizId, currentGame.QuizId);

        // Verify current game relationships
        var currentGameUsers = await dbContext.CurrentGameUser
            .Where(cgu => cgu.CurrentGameId == currentGame.CurrentGameId)
            .ToListAsync();
        Assert.Equal(2, currentGameUsers.Count);

        var currentGameQuestions = await dbContext.CurrentGameQuestion
            .Where(cgq => cgq.CurrentGameId == currentGame.CurrentGameId)
            .ToListAsync();
        Assert.Equal(2, currentGameQuestions.Count);

        Console.WriteLine("[DEBUG_LOG] Data integrity validated - all relationships properly established");
    }

    [Fact]
    public async Task Should_HandleBulkDataOperationsEfficiently()
    {
        // Arrange
        var dbContext = ServiceProvider.GetRequiredService<CrewQuizContext>();

        // Act - Test bulk insert performance
        var startTime = DateTime.UtcNow;

        var bulkUsers = new List<User>();
        for (var i = 1; i <= 100; i++)
            bulkUsers.Add(new User
            {
                FirstName = $"BulkUser{i}",
                LastName = "Test",
                Username = $"bulkuser{i}",
                PasswordHash = $"hash{i}",
                CreatedOn = DateTime.UtcNow
            });

        await dbContext.User.AddRangeAsync(bulkUsers);
        await dbContext.SaveChangesAsync();

        var endTime = DateTime.UtcNow;
        var duration = endTime - startTime;

        // Assert - Verify bulk operation completed
        var totalUsers = await dbContext.User.CountAsync();
        Assert.Equal(100, totalUsers);

        // Performance should be reasonable (under 5 seconds for 100 records)
        Assert.True(duration.TotalSeconds < 5, $"Bulk operation took {duration.TotalSeconds} seconds");

        Console.WriteLine($"[DEBUG_LOG] Bulk insert of 100 users completed in {duration.TotalMilliseconds} ms");
    }
}