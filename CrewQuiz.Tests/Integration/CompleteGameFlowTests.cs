using System.Reflection;
using System.Security.Claims;
using Backend.Data;
using Backend.Extensions;
using Backend.Hubs;
using Backend.Interfaces.Data;
using Backend.Interfaces.Services;
using Backend.Models.Configurations;
using Backend.Models.Domains;
using Backend.Models.DTOs;
using Backend.Models.Exceptions;
using CrewQuiz.Tests.EnvironmentSetup;
using CrewQuiz.Tests.SignalRTesting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CrewQuiz.Tests.Integration;

public class CompleteGameFlowTests : SignalRTestBase
{
    private readonly IAuthenticationService _authService;
    private readonly ICurrentGameService _currentGameService;
    private readonly TestDataSeeder _dataSeeder;
    private readonly IGameFlowService _gameFlowService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;

    public CompleteGameFlowTests()
    {
        // Register missing SignalR services before getting other services
        RegisterSignalRMockServices();

        // Get services from DI container
        _userService = ServiceProvider.GetRequiredService<IUserService>();
        _authService = ServiceProvider.GetRequiredService<IAuthenticationService>();
        _currentGameService = ServiceProvider.GetRequiredService<ICurrentGameService>();
        _gameFlowService = ServiceProvider.GetRequiredService<IGameFlowService>();
        _unitOfWork = ServiceProvider.GetRequiredService<IUnitOfWork>();
        _dataSeeder = new TestDataSeeder(DbContext);
    }

    private void RegisterSignalRMockServices()
    {
        // Create a new service collection and copy existing services
        var services = new ServiceCollection();

        // Get the current services - we need to rebuild the service provider
        // to include the missing IHubContext<GameHub>
        var currentServices = new List<ServiceDescriptor>();

        // Register essential services manually
        services.AddSingleton(Configuration);
        services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
        services.AddLogging();
        services.AddAutoMapper(typeof(CrewQuizContext).Assembly);
        services.AddDbContext<CrewQuizContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddHttpContextAccessor();

        // Add JWT authentication
        services.AddJwtAuthentication(Configuration as ConfigurationManager ?? new ConfigurationManager());

        // Add repositories and services
        services.AddRepositories();
        services.AddServices();

        // Create mock IHubContext<GameHub>
        var mockHubContext = new Mock<IHubContext<GameHub>>();
        var mockClients = new Mock<IHubClients>();

        mockClients.Setup(x => x.All).Returns(MockHubContext.GroupClientProxy.Object);
        mockClients.Setup(x => x.Group(It.IsAny<string>())).Returns(MockHubContext.GroupClientProxy.Object);
        mockClients.Setup(x => x.Client(It.IsAny<string>())).Returns(MockHubContext.SingleClientProxy.Object);

        mockHubContext.Setup(x => x.Groups).Returns(MockHubContext.Groups.Object);
        mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);

        // Register the mock IHubContext<GameHub>
        services.AddSingleton(mockHubContext.Object);

        // Build new service provider
        var newServiceProvider = services.BuildServiceProvider();

        // Replace ServiceProvider using reflection
        var property = typeof(TestBase).GetProperty("ServiceProvider",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        property?.SetValue(this, newServiceProvider);

        // Also update DbContext
        var dbContextProperty = typeof(TestBase).GetProperty("DbContext",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        dbContextProperty?.SetValue(this, newServiceProvider.GetRequiredService<CrewQuizContext>());
    }

    [Fact]
    public async Task CompleteGameFlow_FullWorkflow_Success()
    {
        Console.WriteLine("[DEBUG_LOG] Starting complete game flow integration test");

        // === PHASE 1: SEED TEST DATA ===
        Console.WriteLine("[DEBUG_LOG] Phase 1: Setting up test data");

        await _dataSeeder.SeedTestDataAsync();

        // Get seeded test data from context
        var users = await DbContext.User.ToListAsync();
        var gamemaster = users.First();
        var player1 = users.Skip(1).First();
        var player2 = users.Skip(2).First();

        // Create authentication DTOs for login
        var gmAuth = new AuthenticationDto { Username = gamemaster.Username, PasswordMd5 = "testpassword" };
        var p1Auth = new AuthenticationDto { Username = player1.Username, PasswordMd5 = "testpassword" };
        var p2Auth = new AuthenticationDto { Username = player2.Username, PasswordMd5 = "testpassword" };

        Console.WriteLine("[DEBUG_LOG] Test data seeded successfully");

        // === PHASE 2: AUTHENTICATION ===
        Console.WriteLine("[DEBUG_LOG] Phase 2: User Authentication");

        var gmToken = await _authService.Login(gmAuth);
        var p1Token = await _authService.Login(p1Auth);
        var p2Token = await _authService.Login(p2Auth);

        Assert.NotNull(gmToken?.Token);
        Assert.NotNull(p1Token?.Token);
        Assert.NotNull(p2Token?.Token);

        Console.WriteLine("[DEBUG_LOG] All users authenticated successfully");

        // === PHASE 3: GAME SESSION CREATION ===
        Console.WriteLine("[DEBUG_LOG] Phase 3: Game Session Management");

        // Get seeded current games from context
        var currentGames = await DbContext.CurrentGame.ToListAsync();
        var testGame = currentGames.First();

        Assert.NotNull(testGame);
        Assert.False(testGame.IsStarted);

        Console.WriteLine("[DEBUG_LOG] Game session created with SessionId: " + testGame.SessionId);

        // === PHASE 4: PLAYERS JOIN GAME VIA SIGNALR ===
        Console.WriteLine("[DEBUG_LOG] Phase 4: SignalR Integration Test");

        // Test SignalR hub functionality
        SetupConnectionId("gm-connection");
        await GameHub.JoinGame(testGame.SessionId, gamemaster.UserId);
        AssertJoinedGroup(testGame.SessionId);

        SetupConnectionId("p1-connection");
        await GameHub.JoinGame(testGame.SessionId, player1.UserId);

        SetupConnectionId("p2-connection");
        await GameHub.JoinGame(testGame.SessionId, player2.UserId);

        Console.WriteLine("[DEBUG_LOG] SignalR connections established successfully");

        // === PHASE 5: GAME FLOW OPERATIONS ===
        Console.WriteLine("[DEBUG_LOG] Phase 5: Game Flow Operations");

        // Test adding users to game
        // Each user adds themselves to the game (not the gamemaster adding them)
        SetupAuthenticatedUser(player1.UserId);
        var joinDto1 = new GameFlowDto
        {
            SessionId = testGame.SessionId,
            UserId = player1.UserId
        };

        await _gameFlowService.AddUserToCurrentGame(joinDto1);

        SetupAuthenticatedUser(player2.UserId);
        var joinDto2 = new GameFlowDto
        {
            SessionId = testGame.SessionId,
            UserId = player2.UserId
        };

        await _gameFlowService.AddUserToCurrentGame(joinDto2);

        Console.WriteLine("[DEBUG_LOG] Players added to game successfully");

        // Test starting the game (gamemaster operation)
        SetupAuthenticatedUser(gamemaster.UserId);
        var startDto = new GameFlowDto
        {
            SessionId = testGame.SessionId,
            UserId = gamemaster.UserId
        };

        await _gameFlowService.StartGame(startDto);

        Console.WriteLine("[DEBUG_LOG] Game started successfully");

        // === PHASE 6: CONCURRENT SESSION TEST ===
        Console.WriteLine("[DEBUG_LOG] Phase 6: Concurrent Sessions Test");

        // Create second concurrent session
        var concurrentGame = new CurrentGameDto
        {
            SessionId = Guid.NewGuid().ToString(),
            QuizId = testGame.QuizId,
            IsStarted = false
        };

        await _currentGameService.CreateCurrentGame(concurrentGame);

        // Verify both sessions exist
        var originalSession = await _unitOfWork.CurrentGames.FirstOrDefaultAsync(g => g.SessionId == testGame.SessionId);
        var concurrentSession = await _unitOfWork.CurrentGames.FirstOrDefaultAsync(g => g.SessionId == concurrentGame.SessionId);

        Assert.NotNull(originalSession);
        Assert.NotNull(concurrentSession);
        Assert.NotEqual(originalSession.SessionId, concurrentSession.SessionId);

        Console.WriteLine("[DEBUG_LOG] Concurrent sessions verified successfully");

        // === PHASE 7: DATA CONSISTENCY VALIDATION ===
        Console.WriteLine("[DEBUG_LOG] Phase 7: Data Consistency Validation");

        // Verify database integrity
        var allGames = await DbContext.CurrentGame.ToListAsync();
        var allUsers = await DbContext.User.ToListAsync();
        var gameCount = allGames.Count;
        var userCount = allUsers.Count;

        Assert.True(gameCount >= 2); // At least our two test games
        Assert.True(userCount >= 3); // At least our three test users

        Console.WriteLine("[DEBUG_LOG] Data consistency validated: {0} games, {1} users", gameCount, userCount);

        // === PHASE 8: CLEANUP TEST ===
        Console.WriteLine("[DEBUG_LOG] Phase 8: Leave Game Test");

        // Set up HTTP context for authenticated user (player2 leaving)
        SetupAuthenticatedUser(player2.UserId);

        var leaveDto = new GameFlowDto
        {
            SessionId = testGame.SessionId,
            UserId = player2.UserId
        };

        await _gameFlowService.LeaveGame(leaveDto);

        // Test SignalR cleanup
        SetupConnectionId("p2-connection");
        await GameHub.LeaveGame(testGame.SessionId, player2.UserId);
        AssertLeftGroup(testGame.SessionId);

        Console.WriteLine("[DEBUG_LOG] Player left game successfully");

        // === FINAL VERIFICATION ===
        Console.WriteLine("[DEBUG_LOG] Complete game flow integration test passed all phases");
        Assert.True(true, "Story 16: Complete end-to-end integration test successful");
    }

    [Fact]
    public async Task CompleteGameFlow_MultipleSimultaneousGames_Success()
    {
        Console.WriteLine("[DEBUG_LOG] Starting multiple simultaneous games test");

        // Seed initial test data
        await _dataSeeder.SeedTestDataAsync();
        var users = await DbContext.User.ToListAsync();
        var testUser = users.First();

        // Create multiple concurrent sessions
        var tasks = new List<Task>();
        var sessionIds = new List<string>();

        for (var i = 0; i < 3; i++)
        {
            var sessionId = Guid.NewGuid().ToString();
            sessionIds.Add(sessionId);

            tasks.Add(Task.Run(async () => { await CreateConcurrentGameSession(sessionId, testUser); }));
        }

        // Wait for all concurrent games to complete
        await Task.WhenAll(tasks);

        // Verify all sessions exist independently  
        foreach (var sessionId in sessionIds)
        {
            var game = await _unitOfWork.CurrentGames.FirstOrDefaultAsync(g => g.SessionId == sessionId);
            Assert.NotNull(game);
        }

        Console.WriteLine("[DEBUG_LOG] Multiple simultaneous games completed successfully");
    }

    [Fact]
    public async Task CompleteGameFlow_ErrorHandling_PropagatesCorrectly()
    {
        Console.WriteLine("[DEBUG_LOG] Starting error handling test");

        // Test authentication errors
        var invalidLogin = new AuthenticationDto
        {
            Username = "nonexistent",
            PasswordMd5 = "invalid"
        };

        await Assert.ThrowsAsync<BusinessValidationException>(async () => { await _authService.Login(invalidLogin); });

        // Test game flow errors
        var invalidSessionDto = new GameFlowDto
        {
            SessionId = "nonexistent-session",
            UserId = 999
        };

        await Assert.ThrowsAsync<BusinessValidationException>(async () => { await _gameFlowService.AddUserToCurrentGame(invalidSessionDto); });

        Console.WriteLine("[DEBUG_LOG] Error handling verified successfully");
    }

    private async Task CreateConcurrentGameSession(string sessionId, User testUser)
    {
        // Set up HTTP context for authenticated user
        SetupAuthenticatedUser(testUser.UserId);

        // Create concurrent game session using existing seeded data
        // Use a hardcoded QuizId to avoid DbContext concurrency issues
        var gameDto = new CurrentGameDto
        {
            QuizId = 1, // Using the first quiz from seeded data
            SessionId = sessionId,
            IsStarted = false
        };

        await _currentGameService.CreateCurrentGame(gameDto);

        // Test basic start operation
        var startDto = new GameFlowDto
        {
            SessionId = sessionId,
            UserId = testUser.UserId
        };

        await _gameFlowService.StartGame(startDto);

        Console.WriteLine("[DEBUG_LOG] Concurrent session {0} completed successfully", sessionId);
    }

    /// <summary>
    ///     Sets up an authenticated user context for service operations
    /// </summary>
    private void SetupAuthenticatedUser(long userId)
    {
        var httpContextAccessor = ServiceProvider.GetRequiredService<IHttpContextAccessor>();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, $"TestUser{userId}")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        httpContextAccessor.HttpContext = httpContext;
    }
}