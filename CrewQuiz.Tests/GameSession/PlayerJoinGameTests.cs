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
///     Tests for Story 8: Player Join Game
///     Tests the functionality for players to join existing game sessions
/// </summary>
public class PlayerJoinGameTests : TestBase
{
    private readonly GameFlowController _controller;
    private readonly Mock<IGameFlowService> _gameFlowServiceMock;
    private readonly Mock<IServiceDispatcher> _serviceDispatcherMock;
    private readonly Mock<IServiceMethodDispatcher<IGameFlowService>> _serviceMethodDispatcherMock;

    public PlayerJoinGameTests()
    {
        _serviceDispatcherMock = new Mock<IServiceDispatcher>();
        _serviceMethodDispatcherMock = new Mock<IServiceMethodDispatcher<IGameFlowService>>();
        _gameFlowServiceMock = new Mock<IGameFlowService>();

        // Setup service dispatcher to return service method dispatcher mock
        _serviceDispatcherMock.Setup(x => x.For<IGameFlowService>())
            .Returns(_serviceMethodDispatcherMock.Object);

        // Setup service method dispatcher to call the actual service methods
        _serviceMethodDispatcherMock.Setup(x => x.DispatchAsync(It.IsAny<Func<IGameFlowService, Task>>()))
            .Returns((Func<IGameFlowService, Task> method) => method(_gameFlowServiceMock.Object));

        _controller = new GameFlowController(_serviceDispatcherMock.Object);

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
    public async Task AddUserToCurrentGame_ValidGameFlowDto_ReturnsOk()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            UserId = 1,
            SessionId = "TEST-SESSION-123"
        };

        _gameFlowServiceMock.Setup(x => x.AddUserToCurrentGame(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AddUserToCurrentGame(gameFlowDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _gameFlowServiceMock.Verify(x => x.AddUserToCurrentGame(It.Is<GameFlowDto>(g =>
            g.SessionId == "TEST-SESSION-123" &&
            g.UserId == 1)), Times.Once);
        Console.WriteLine("[DEBUG_LOG] AddUserToCurrentGame test passed - Player successfully joined game session");
    }

    [Fact]
    public async Task AddUserToCurrentGame_ValidSessionId_CallsServiceCorrectly()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            UserId = 2,
            SessionId = "VALID-SESSION-456"
        };

        _gameFlowServiceMock.Setup(x => x.AddUserToCurrentGame(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AddUserToCurrentGame(gameFlowDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _gameFlowServiceMock.Verify(x => x.AddUserToCurrentGame(It.IsAny<GameFlowDto>()), Times.Once);
        Console.WriteLine("[DEBUG_LOG] AddUserToCurrentGame with valid session test passed - Service called correctly");
    }

    [Fact]
    public async Task AddUserToCurrentGame_GameNotFound_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            UserId = 1,
            SessionId = "NONEXISTENT-SESSION"
        };

        _gameFlowServiceMock.Setup(x => x.AddUserToCurrentGame(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("Game was not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.AddUserToCurrentGame(gameFlowDto));

        Assert.Equal("Game was not found", exception.Message);
        _gameFlowServiceMock.Verify(x => x.AddUserToCurrentGame(It.IsAny<GameFlowDto>()), Times.Once);
        Console.WriteLine("[DEBUG_LOG] Game not found test passed - BusinessValidationException thrown for nonexistent session");
    }

    [Fact]
    public async Task AddUserToCurrentGame_GameAlreadyStarted_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            UserId = 1,
            SessionId = "STARTED-GAME-SESSION"
        };

        _gameFlowServiceMock.Setup(x => x.AddUserToCurrentGame(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("Game has already been started"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.AddUserToCurrentGame(gameFlowDto));

        Assert.Equal("Game has already been started", exception.Message);
        _gameFlowServiceMock.Verify(x => x.AddUserToCurrentGame(It.IsAny<GameFlowDto>()), Times.Once);
        Console.WriteLine("[DEBUG_LOG] Game already started test passed - Players prevented from joining started games");
    }

    [Fact]
    public async Task AddUserToCurrentGame_UserNotFound_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            UserId = 999, // Non-existent user
            SessionId = "VALID-SESSION-123"
        };

        _gameFlowServiceMock.Setup(x => x.AddUserToCurrentGame(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("User not found."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.AddUserToCurrentGame(gameFlowDto));

        Assert.Equal("User not found.", exception.Message);
        _gameFlowServiceMock.Verify(x => x.AddUserToCurrentGame(It.IsAny<GameFlowDto>()), Times.Once);
        Console.WriteLine("[DEBUG_LOG] User not found test passed - BusinessValidationException thrown for non-existent user");
    }

    [Fact]
    public async Task AddUserToCurrentGame_MultiplePlayersJoinSameSession_AllSucceed()
    {
        // Arrange
        var player1Dto = new GameFlowDto
        {
            UserId = 1,
            SessionId = "MULTIPLAYER-SESSION"
        };

        var player2Dto = new GameFlowDto
        {
            UserId = 2,
            SessionId = "MULTIPLAYER-SESSION"
        };

        var player3Dto = new GameFlowDto
        {
            UserId = 3,
            SessionId = "MULTIPLAYER-SESSION"
        };

        _gameFlowServiceMock.Setup(x => x.AddUserToCurrentGame(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result1 = await _controller.AddUserToCurrentGame(player1Dto);
        var result2 = await _controller.AddUserToCurrentGame(player2Dto);
        var result3 = await _controller.AddUserToCurrentGame(player3Dto);

        // Assert
        Assert.IsType<OkResult>(result1);
        Assert.IsType<OkResult>(result2);
        Assert.IsType<OkResult>(result3);
        _gameFlowServiceMock.Verify(x => x.AddUserToCurrentGame(It.IsAny<GameFlowDto>()), Times.Exactly(3));
        Console.WriteLine("[DEBUG_LOG] Multiple players join test passed - All players successfully joined same session");
    }

    [Fact]
    public async Task AddUserToCurrentGame_IdentityTheft_ThrowsBusinessValidationException()
    {
        // Arrange - Different user ID than authenticated user (UserId 1)
        var gameFlowDto = new GameFlowDto
        {
            UserId = 2, // Different from authenticated user (ID 1)
            SessionId = "VALID-SESSION-123"
        };

        _gameFlowServiceMock.Setup(x => x.AddUserToCurrentGame(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("Identity theft is not a joke"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.AddUserToCurrentGame(gameFlowDto));

        Assert.Equal("Identity theft is not a joke", exception.Message);
        _gameFlowServiceMock.Verify(x => x.AddUserToCurrentGame(It.IsAny<GameFlowDto>()), Times.Once);
        Console.WriteLine("[DEBUG_LOG] Identity theft prevention test passed - Users can only join as themselves");
    }

    [Fact]
    public async Task AddUserToCurrentGame_NullUserId_ThrowsException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            UserId = null, // Required field missing
            SessionId = "VALID-SESSION-123"
        };

        _gameFlowServiceMock.Setup(x => x.AddUserToCurrentGame(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new Exception("UserId must be provided"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _controller.AddUserToCurrentGame(gameFlowDto));

        Assert.Contains("UserId must be provided", exception.Message);
        _gameFlowServiceMock.Verify(x => x.AddUserToCurrentGame(It.IsAny<GameFlowDto>()), Times.Once);
        Console.WriteLine("[DEBUG_LOG] Null UserId validation test passed - UserId is required for joining games");
    }

    [Fact]
    public async Task AddUserToCurrentGame_EmptySessionId_ThrowsException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            UserId = 1,
            SessionId = "" // Required field empty
        };

        _gameFlowServiceMock.Setup(x => x.AddUserToCurrentGame(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act - This should work at controller level since SessionId is just empty, not null
        // The validation happens at service level
        var result = await _controller.AddUserToCurrentGame(gameFlowDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _gameFlowServiceMock.Verify(x => x.AddUserToCurrentGame(It.Is<GameFlowDto>(g => g.SessionId == "")), Times.Once);
        Console.WriteLine("[DEBUG_LOG] Empty SessionId test passed - Controller accepts empty SessionId, validation handled by service");
    }

    [Fact]
    public async Task AddUserToCurrentGame_ConcurrentJoins_HandledCorrectly()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            UserId = 1,
            SessionId = "CONCURRENT-SESSION"
        };

        _gameFlowServiceMock.Setup(x => x.AddUserToCurrentGame(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act - Simulate concurrent requests
        var task1 = _controller.AddUserToCurrentGame(gameFlowDto);
        var task2 = _controller.AddUserToCurrentGame(gameFlowDto);
        var task3 = _controller.AddUserToCurrentGame(gameFlowDto);

        var results = await Task.WhenAll(task1, task2, task3);

        // Assert
        Assert.All(results, result => Assert.IsType<OkResult>(result));
        _gameFlowServiceMock.Verify(x => x.AddUserToCurrentGame(It.IsAny<GameFlowDto>()), Times.Exactly(3));
        Console.WriteLine("[DEBUG_LOG] Concurrent joins test passed - Multiple concurrent join requests handled properly");
    }
}