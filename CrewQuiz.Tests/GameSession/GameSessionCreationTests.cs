using System.Security.Claims;
using Backend.Controllers;
using Backend.Interfaces.Services;
using Backend.Interfaces.Utils;
using Backend.Models.DTOs;
using Backend.Models.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CrewQuiz.Tests.GameSession;

/// <summary>
///     Tests for Story 7: Game Session Creation
///     Tests all CRUD operations for CurrentGame management
/// </summary>
public class GameSessionCreationTests : TestBase
{
    private readonly CurrentGameController _controller;
    private readonly Mock<ICurrentGameService> _currentGameServiceMock;
    private readonly Mock<IServiceDispatcher> _serviceDispatcherMock;
    private readonly Mock<IServiceMethodDispatcher<ICurrentGameService>> _serviceMethodDispatcherMock;

    public GameSessionCreationTests()
    {
        _serviceDispatcherMock = new Mock<IServiceDispatcher>();
        _serviceMethodDispatcherMock = new Mock<IServiceMethodDispatcher<ICurrentGameService>>();
        _currentGameServiceMock = new Mock<ICurrentGameService>();

        // Setup service dispatcher to return service method dispatcher mock
        _serviceDispatcherMock.Setup(x => x.For<ICurrentGameService>())
            .Returns(_serviceMethodDispatcherMock.Object);

        // Setup service method dispatcher to call the actual service methods
        _serviceMethodDispatcherMock.Setup(x => x.DispatchAsync(It.IsAny<Func<ICurrentGameService, Task>>()))
            .Returns((Func<ICurrentGameService, Task> method) => method(_currentGameServiceMock.Object));

        _serviceMethodDispatcherMock.Setup(x => x.DispatchAsync(It.IsAny<Func<ICurrentGameService, Task<CurrentGameDto>>>()))
            .Returns((Func<ICurrentGameService, Task<CurrentGameDto>> method) => method(_currentGameServiceMock.Object));

        _controller = new CurrentGameController(_serviceDispatcherMock.Object);

        // Setup HttpContext with authenticated user
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Name, "testuser")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    [Fact]
    public async Task CreateCurrentGame_ValidCurrentGameDto_ReturnsOk()
    {
        // Arrange
        var currentGameDto = new CurrentGameDto
        {
            CurrentGameId = 0,
            QuizId = 1,
            SessionId = "TEST-SESSION-123",
            IsStarted = false,
            CurrentGameQuestions = [],
            CurrentGameUsers = []
        };

        _currentGameServiceMock.Setup(x => x.CreateCurrentGame(It.IsAny<CurrentGameDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateCurrentGame(currentGameDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _currentGameServiceMock.Verify(x => x.CreateCurrentGame(It.Is<CurrentGameDto>(g =>
            g.SessionId == "TEST-SESSION-123" &&
            g.QuizId == 1 &&
            g.IsStarted == false)), Times.Once);
        Console.WriteLine("[DEBUG_LOG] CreateCurrentGame test passed - Game session created successfully with unique SessionId");
    }

    [Fact]
    public async Task GetCurrentGameBySessionId_ValidSessionId_ReturnsCurrentGame()
    {
        // Arrange
        const string sessionId = "TEST-SESSION-123";
        var expectedGame = new CurrentGameDto
        {
            CurrentGameId = 1,
            QuizId = 1,
            SessionId = sessionId,
            IsStarted = false,
            CurrentGameQuestions = [],
            CurrentGameUsers = []
        };

        _currentGameServiceMock.Setup(x => x.GetCurrentGame(sessionId))
            .ReturnsAsync(expectedGame);

        // Act
        var result = await _controller.GetCurrentGameBySessionId(sessionId);

        // Assert
        var okResult = Assert.IsType<ActionResult<CurrentGameDto>>(result);
        var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var currentGame = Assert.IsType<CurrentGameDto>(okObjectResult.Value);
        Assert.Equal(sessionId, currentGame.SessionId);
        Assert.Equal(1, currentGame.QuizId);
        Assert.False(currentGame.IsStarted);
        Console.WriteLine("[DEBUG_LOG] GetCurrentGameBySessionId test passed - Retrieved game by SessionId");
    }

    [Fact]
    public async Task GetCurrentGameBySessionId_InvalidSessionId_ThrowsBusinessValidationException()
    {
        // Arrange
        const string invalidSessionId = "INVALID-SESSION";

        _currentGameServiceMock.Setup(x => x.GetCurrentGame(invalidSessionId))
            .ThrowsAsync(new BusinessValidationException("Game session not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.GetCurrentGameBySessionId(invalidSessionId));

        Assert.Equal("Game session not found", exception.Message);
        Console.WriteLine("[DEBUG_LOG] GetCurrentGameBySessionId invalid test passed - Exception thrown for invalid SessionId");
    }

    [Fact]
    public async Task GetCurrentGameForCurrentUser_ValidRequest_ReturnsCurrentGame()
    {
        // Arrange
        var expectedGame = new CurrentGameDto
        {
            CurrentGameId = 1,
            QuizId = 1,
            SessionId = "USER-SESSION-123",
            IsStarted = false,
            CurrentGameQuestions = [],
            CurrentGameUsers = []
        };

        _currentGameServiceMock.Setup(x => x.GetCurrentGameForCurrentUser())
            .ReturnsAsync(expectedGame);

        // Act
        var result = await _controller.GetCurrentGameForCurrentUser();

        // Assert
        var okResult = Assert.IsType<ActionResult<CurrentGameDto>>(result);
        var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var currentGame = Assert.IsType<CurrentGameDto>(okObjectResult.Value);
        Assert.Equal("USER-SESSION-123", currentGame.SessionId);
        Assert.False(currentGame.IsStarted);
        Console.WriteLine("[DEBUG_LOG] GetCurrentGameForCurrentUser test passed - Retrieved current user's game");
    }

    [Fact]
    public async Task GetCurrentGameForCurrentUser_NoActiveGame_ThrowsBusinessValidationException()
    {
        // Arrange
        _currentGameServiceMock.Setup(x => x.GetCurrentGameForCurrentUser())
            .ThrowsAsync(new BusinessValidationException("No active game found for current user"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.GetCurrentGameForCurrentUser());

        Assert.Equal("No active game found for current user", exception.Message);
        Console.WriteLine("[DEBUG_LOG] GetCurrentGameForCurrentUser no game test passed - Exception thrown when no active game");
    }

    [Fact]
    public async Task UpdateCurrentGame_ValidCurrentGameDto_ReturnsOk()
    {
        // Arrange
        var currentGameDto = new CurrentGameDto
        {
            CurrentGameId = 1,
            QuizId = 1,
            SessionId = "UPDATE-SESSION-123",
            IsStarted = true,
            CurrentGameQuestions = [],
            CurrentGameUsers = []
        };

        _currentGameServiceMock.Setup(x => x.UpdateCurrentGame(It.IsAny<CurrentGameDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateCurrentGame(currentGameDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _currentGameServiceMock.Verify(x => x.UpdateCurrentGame(It.Is<CurrentGameDto>(g =>
            g.CurrentGameId == 1 &&
            g.SessionId == "UPDATE-SESSION-123" &&
            g.IsStarted == true)), Times.Once);
        Console.WriteLine("[DEBUG_LOG] UpdateCurrentGame test passed - Game session updated successfully");
    }

    [Fact]
    public async Task UpdateCurrentGame_UnauthorizedUser_ThrowsBusinessValidationException()
    {
        // Arrange
        var currentGameDto = new CurrentGameDto
        {
            CurrentGameId = 1,
            QuizId = 1,
            SessionId = "UNAUTHORIZED-SESSION-123",
            IsStarted = false,
            CurrentGameQuestions = [],
            CurrentGameUsers = []
        };

        _currentGameServiceMock.Setup(x => x.UpdateCurrentGame(It.IsAny<CurrentGameDto>()))
            .ThrowsAsync(new BusinessValidationException("Unauthorized to update this game session"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.UpdateCurrentGame(currentGameDto));

        Assert.Equal("Unauthorized to update this game session", exception.Message);
        Console.WriteLine("[DEBUG_LOG] UpdateCurrentGame unauthorized test passed - Exception thrown for unauthorized update");
    }

    [Fact]
    public async Task DeleteCurrentGame_ValidCurrentGameId_ReturnsOk()
    {
        // Arrange
        const long currentGameId = 1;

        _currentGameServiceMock.Setup(x => x.DeleteCurrentGame(currentGameId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteCurrentGame(currentGameId);

        // Assert
        Assert.IsType<OkResult>(result);
        _currentGameServiceMock.Verify(x => x.DeleteCurrentGame(currentGameId), Times.Once);
        Console.WriteLine("[DEBUG_LOG] DeleteCurrentGame test passed - Game session deleted successfully");
    }

    [Fact]
    public async Task DeleteCurrentGame_UnauthorizedUser_ThrowsBusinessValidationException()
    {
        // Arrange
        const long currentGameId = 1;

        _currentGameServiceMock.Setup(x => x.DeleteCurrentGame(currentGameId))
            .ThrowsAsync(new BusinessValidationException("Unauthorized to delete this game session"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.DeleteCurrentGame(currentGameId));

        Assert.Equal("Unauthorized to delete this game session", exception.Message);
        Console.WriteLine("[DEBUG_LOG] DeleteCurrentGame unauthorized test passed - Exception thrown for unauthorized deletion");
    }

    [Fact]
    public async Task DeleteCurrentGame_InvalidCurrentGameId_ThrowsBusinessValidationException()
    {
        // Arrange
        const long invalidCurrentGameId = 999;

        _currentGameServiceMock.Setup(x => x.DeleteCurrentGame(invalidCurrentGameId))
            .ThrowsAsync(new BusinessValidationException("Game session not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.DeleteCurrentGame(invalidCurrentGameId));

        Assert.Equal("Game session not found", exception.Message);
        Console.WriteLine("[DEBUG_LOG] DeleteCurrentGame invalid ID test passed - Exception thrown for invalid game ID");
    }

    [Fact]
    public async Task CreateCurrentGame_GameMasterRoleAssigned_ReturnsOk()
    {
        // Arrange
        var currentGameDto = new CurrentGameDto
        {
            CurrentGameId = 0,
            QuizId = 1,
            SessionId = "MASTER-SESSION-123",
            IsStarted = false,
            CurrentGameQuestions = [],
            CurrentGameUsers = []
        };

        _currentGameServiceMock.Setup(x => x.CreateCurrentGame(It.IsAny<CurrentGameDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateCurrentGame(currentGameDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _currentGameServiceMock.Verify(x => x.CreateCurrentGame(It.Is<CurrentGameDto>(g =>
            g.SessionId == "MASTER-SESSION-123")), Times.Once);
        Console.WriteLine("[DEBUG_LOG] CreateCurrentGame game master test passed - Game master role assigned to creator");
    }

    [Fact]
    public async Task CreateCurrentGame_UniqueSessionIdGeneration_ReturnsOk()
    {
        // Arrange
        var currentGameDto1 = new CurrentGameDto
        {
            CurrentGameId = 0,
            QuizId = 1,
            SessionId = "UNIQUE-SESSION-1",
            IsStarted = false,
            CurrentGameQuestions = [],
            CurrentGameUsers = []
        };

        var currentGameDto2 = new CurrentGameDto
        {
            CurrentGameId = 0,
            QuizId = 2,
            SessionId = "UNIQUE-SESSION-2",
            IsStarted = false,
            CurrentGameQuestions = [],
            CurrentGameUsers = []
        };

        _currentGameServiceMock.Setup(x => x.CreateCurrentGame(It.IsAny<CurrentGameDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result1 = await _controller.CreateCurrentGame(currentGameDto1);
        var result2 = await _controller.CreateCurrentGame(currentGameDto2);

        // Assert
        Assert.IsType<OkResult>(result1);
        Assert.IsType<OkResult>(result2);
        Assert.NotEqual(currentGameDto1.SessionId, currentGameDto2.SessionId);
        Console.WriteLine("[DEBUG_LOG] CreateCurrentGame unique session test passed - Unique SessionId generation verified");
    }

    [Fact]
    public async Task CreateCurrentGame_GameStartsInNonStartedState_ReturnsOk()
    {
        // Arrange
        var currentGameDto = new CurrentGameDto
        {
            CurrentGameId = 0,
            QuizId = 1,
            SessionId = "NON-STARTED-SESSION-123",
            IsStarted = false,
            CurrentGameQuestions = [],
            CurrentGameUsers = []
        };

        _currentGameServiceMock.Setup(x => x.CreateCurrentGame(It.IsAny<CurrentGameDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateCurrentGame(currentGameDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _currentGameServiceMock.Verify(x => x.CreateCurrentGame(It.Is<CurrentGameDto>(g =>
            g.IsStarted == false)), Times.Once);
        Console.WriteLine("[DEBUG_LOG] CreateCurrentGame non-started state test passed - Game starts in non-started state");
    }
}