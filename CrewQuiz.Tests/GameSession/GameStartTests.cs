using Backend.Controllers;
using Backend.Interfaces.Services;
using Backend.Interfaces.Utils;
using Backend.Models.DTOs;
using Backend.Models.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CrewQuiz.Tests.GameSession;

public class GameStartTests
{
    private readonly GameFlowController _controller;
    private readonly Mock<IGameFlowService> _gameFlowServiceMock;
    private readonly Mock<IServiceDispatcher> _serviceDispatcherMock;
    private readonly Mock<IServiceMethodDispatcher<IGameFlowService>> _serviceMethodDispatcherMock;

    public GameStartTests()
    {
        _serviceDispatcherMock = new Mock<IServiceDispatcher>();
        _serviceMethodDispatcherMock = new Mock<IServiceMethodDispatcher<IGameFlowService>>();
        _gameFlowServiceMock = new Mock<IGameFlowService>();

        _serviceDispatcherMock.Setup(x => x.For<IGameFlowService>())
            .Returns(_serviceMethodDispatcherMock.Object);

        _serviceMethodDispatcherMock.Setup(x => x.DispatchAsync(It.IsAny<Func<IGameFlowService, Task>>()))
            .Returns((Func<IGameFlowService, Task> method) => method(_gameFlowServiceMock.Object));

        _controller = new GameFlowController(_serviceDispatcherMock.Object);
    }

    [Fact]
    public async Task StartGame_ValidGameFlowDto_ReturnsOk()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-123"
        };

        _gameFlowServiceMock.Setup(x => x.StartGame(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.StartGame(gameFlowDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _gameFlowServiceMock.Verify(x => x.StartGame(It.Is<GameFlowDto>(g =>
            g.SessionId == "TEST-SESSION-123")), Times.Once);
    }

    [Fact]
    public async Task StartGame_ValidSessionId_CallsServiceCorrectly()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "GAME-456"
        };

        _gameFlowServiceMock.Setup(x => x.StartGame(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.StartGame(gameFlowDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _gameFlowServiceMock.Verify(x => x.StartGame(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task StartGame_GameNotFound_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "NONEXISTENT-GAME"
        };

        _gameFlowServiceMock.Setup(x => x.StartGame(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("Game was not found"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.StartGame(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.StartGame(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task StartGame_GameAlreadyStarted_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "ALREADY-STARTED-GAME"
        };

        _gameFlowServiceMock.Setup(x => x.StartGame(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("Game has already been started"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.StartGame(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.StartGame(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task StartGame_NotGameMaster_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION"
        };

        _gameFlowServiceMock.Setup(x => x.StartGame(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("Only game master can start the game"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.StartGame(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.StartGame(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task StartGame_UserNotInGame_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION"
        };

        _gameFlowServiceMock.Setup(x => x.StartGame(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("User does not belong to this game"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.StartGame(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.StartGame(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task StartGame_EmptySessionId_ThrowsException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = ""
        };

        _gameFlowServiceMock.Setup(x => x.StartGame(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("Session ID is required"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.StartGame(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.StartGame(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task StartGame_MultipleGameMasters_OnlyOneCanStart()
    {
        // Arrange
        var gameFlowDto1 = new GameFlowDto
        {
            SessionId = "MULTI-MASTER-GAME"
        };

        var gameFlowDto2 = new GameFlowDto
        {
            SessionId = "MULTI-MASTER-GAME"
        };

        _gameFlowServiceMock.SetupSequence(x => x.StartGame(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask)
            .ThrowsAsync(new BusinessValidationException("Game has already been started"));

        // Act
        var result1 = await _controller.StartGame(gameFlowDto1);

        // Assert first call succeeds
        Assert.IsType<OkResult>(result1);

        // Assert second call fails
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.StartGame(gameFlowDto2));

        _gameFlowServiceMock.Verify(x => x.StartGame(It.IsAny<GameFlowDto>()), Times.Exactly(2));
    }
}