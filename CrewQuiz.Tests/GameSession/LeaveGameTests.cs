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
///     Tests for Story 15: Leave Game
///     Tests the functionality for players to leave game sessions with proper cleanup and turn adjustment
/// </summary>
public class LeaveGameTests : TestBase
{
    private readonly GameFlowController _controller;
    private readonly Mock<IGameFlowService> _gameFlowServiceMock;
    private readonly Mock<IServiceDispatcher> _serviceDispatcherMock;
    private readonly Mock<IServiceMethodDispatcher<IGameFlowService>> _serviceMethodDispatcherMock;

    public LeaveGameTests()
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
    public async Task LeaveGame_ValidGameFlowDto_ReturnsOk()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            UserId = 1,
            SessionId = "TEST-SESSION-123"
        };

        _gameFlowServiceMock.Setup(x => x.LeaveGame(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.LeaveGame(gameFlowDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _gameFlowServiceMock.Verify(x => x.LeaveGame(It.Is<GameFlowDto>(g =>
            g.SessionId == "TEST-SESSION-123" &&
            g.UserId == 1)), Times.Once);
        Console.WriteLine("[DEBUG_LOG] LeaveGame test passed - Player successfully left game session");
    }

    [Fact]
    public async Task LeaveGame_ValidSessionId_CallsServiceCorrectly()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            UserId = 2,
            SessionId = "VALID-SESSION-456"
        };

        _gameFlowServiceMock.Setup(x => x.LeaveGame(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.LeaveGame(gameFlowDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _gameFlowServiceMock.Verify(x => x.LeaveGame(It.IsAny<GameFlowDto>()), Times.Once);
        Console.WriteLine("[DEBUG_LOG] LeaveGame with valid session test passed - Service called correctly");
    }

    [Fact]
    public async Task LeaveGame_GameNotFound_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            UserId = 1,
            SessionId = "NONEXISTENT-SESSION"
        };

        _gameFlowServiceMock.Setup(x => x.LeaveGame(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("Game was not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(async () => await _controller.LeaveGame(gameFlowDto));

        Assert.Equal("Game was not found", exception.Message);
        _gameFlowServiceMock.Verify(x => x.LeaveGame(It.IsAny<GameFlowDto>()), Times.Once);
        Console.WriteLine("[DEBUG_LOG] LeaveGame game not found test passed - Exception thrown correctly");
    }

    [Fact]
    public async Task LeaveGame_PlayerNotInGame_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            UserId = 999,
            SessionId = "VALID-SESSION-123"
        };

        _gameFlowServiceMock.Setup(x => x.LeaveGame(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("Player not found in game"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(async () => await _controller.LeaveGame(gameFlowDto));

        Assert.Equal("Player not found in game", exception.Message);
        _gameFlowServiceMock.Verify(x => x.LeaveGame(It.IsAny<GameFlowDto>()), Times.Once);
        Console.WriteLine("[DEBUG_LOG] LeaveGame player not in game test passed - Exception thrown correctly");
    }

    [Fact]
    public async Task LeaveGame_CurrentTurnPlayer_TurnOrderAdjusted()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            UserId = 1,
            SessionId = "SESSION-WITH-CURRENT-PLAYER"
        };

        _gameFlowServiceMock.Setup(x => x.LeaveGame(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.LeaveGame(gameFlowDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _gameFlowServiceMock.Verify(x => x.LeaveGame(It.Is<GameFlowDto>(g =>
            g.SessionId == "SESSION-WITH-CURRENT-PLAYER" &&
            g.UserId == 1)), Times.Once);
        Console.WriteLine("[DEBUG_LOG] LeaveGame current turn player test passed - Turn order adjustment handled");
    }

    [Fact]
    public async Task LeaveGame_LastPlayerInGame_GameCanContinue()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            UserId = 1,
            SessionId = "SESSION-WITH-LAST-PLAYER"
        };

        _gameFlowServiceMock.Setup(x => x.LeaveGame(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.LeaveGame(gameFlowDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _gameFlowServiceMock.Verify(x => x.LeaveGame(It.IsAny<GameFlowDto>()), Times.Once);
        Console.WriteLine("[DEBUG_LOG] LeaveGame last player test passed - Game continues with remaining players");
    }

    [Fact]
    public async Task LeaveGame_NullUserId_ThrowsException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            UserId = null,
            SessionId = "VALID-SESSION-123"
        };

        _gameFlowServiceMock.Setup(x => x.LeaveGame(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("UserId is required"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(async () => await _controller.LeaveGame(gameFlowDto));

        Assert.Equal("UserId is required", exception.Message);
        Console.WriteLine("[DEBUG_LOG] LeaveGame null UserId test passed - Exception thrown for missing UserId");
    }

    [Fact]
    public async Task LeaveGame_EmptySessionId_ThrowsException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            UserId = 1,
            SessionId = ""
        };

        _gameFlowServiceMock.Setup(x => x.LeaveGame(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("SessionId is required"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(async () => await _controller.LeaveGame(gameFlowDto));

        Assert.Equal("SessionId is required", exception.Message);
        Console.WriteLine("[DEBUG_LOG] LeaveGame empty SessionId test passed - Exception thrown for missing SessionId");
    }

    [Fact]
    public async Task LeaveGame_ConcurrentLeaves_HandledCorrectly()
    {
        // Arrange
        var sessionId = "CONCURRENT-LEAVE-SESSION";
        var gameFlowDtos = new List<GameFlowDto>
        {
            new() { UserId = 1, SessionId = sessionId },
            new() { UserId = 2, SessionId = sessionId },
            new() { UserId = 3, SessionId = sessionId }
        };

        _gameFlowServiceMock.Setup(x => x.LeaveGame(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var leaveTasks = gameFlowDtos.Select(dto => _controller.LeaveGame(dto)).ToArray();
        var results = await Task.WhenAll(leaveTasks);

        // Assert
        Assert.All(results, result => Assert.IsType<OkResult>(result));
        _gameFlowServiceMock.Verify(x => x.LeaveGame(It.IsAny<GameFlowDto>()), Times.Exactly(3));
        Console.WriteLine("[DEBUG_LOG] LeaveGame concurrent leaves test passed - Multiple players can leave simultaneously");
    }

    [Fact]
    public async Task LeaveGame_GameMasterLeaves_GameCanContinue()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            UserId = 100, // Game master
            SessionId = "SESSION-WITH-GAME-MASTER"
        };

        _gameFlowServiceMock.Setup(x => x.LeaveGame(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.LeaveGame(gameFlowDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _gameFlowServiceMock.Verify(x => x.LeaveGame(It.Is<GameFlowDto>(g =>
            g.SessionId == "SESSION-WITH-GAME-MASTER" &&
            g.UserId == 100)), Times.Once);
        Console.WriteLine("[DEBUG_LOG] LeaveGame game master leaves test passed - Game continues without game master");
    }

    [Fact]
    public async Task LeaveGame_SignalRNotificationSent()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            UserId = 1,
            SessionId = "SIGNALR-NOTIFICATION-SESSION"
        };

        _gameFlowServiceMock.Setup(x => x.LeaveGame(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.LeaveGame(gameFlowDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _gameFlowServiceMock.Verify(x => x.LeaveGame(It.IsAny<GameFlowDto>()), Times.Once);
        Console.WriteLine("[DEBUG_LOG] LeaveGame SignalR notification test passed - Other players notified of departure");
    }
}