using System.Data;
using Backend.Controllers;
using Backend.Interfaces.Services;
using Backend.Interfaces.Utils;
using Backend.Models.DTOs;
using Backend.Models.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CrewQuiz.Tests.GameSession;

public class QuestionSelectionTests
{
    private readonly GameFlowController _controller;
    private readonly Mock<IGameFlowService> _gameFlowServiceMock;
    private readonly Mock<IServiceDispatcher> _serviceDispatcherMock;
    private readonly Mock<IServiceMethodDispatcher<IGameFlowService>> _serviceMethodDispatcherMock;

    public QuestionSelectionTests()
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
    public async Task SelectQuestion_ValidQuestionSelection_ReturnsOk()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-123",
            UserId = 1,
            QuestionId = 100
        };

        _gameFlowServiceMock.Setup(x => x.SelectQuestion(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.SelectQuestion(gameFlowDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _gameFlowServiceMock.Verify(x => x.SelectQuestion(It.Is<GameFlowDto>(g =>
            g.SessionId == "TEST-SESSION-123" &&
            g.UserId == 1 &&
            g.QuestionId == 100)), Times.Once);
    }

    [Fact]
    public async Task SelectQuestion_ValidSessionAndQuestionId_CallsServiceCorrectly()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "GAME-456",
            UserId = 2,
            QuestionId = 200
        };

        _gameFlowServiceMock.Setup(x => x.SelectQuestion(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.SelectQuestion(gameFlowDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _gameFlowServiceMock.Verify(x => x.SelectQuestion(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task SelectQuestion_GameNotFound_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "NONEXISTENT-GAME",
            UserId = 1,
            QuestionId = 100
        };

        _gameFlowServiceMock.Setup(x => x.SelectQuestion(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("Game was not found"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.SelectQuestion(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.SelectQuestion(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task SelectQuestion_UserNotInGame_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-123",
            UserId = 999, // User not in game
            QuestionId = 100
        };

        _gameFlowServiceMock.Setup(x => x.SelectQuestion(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("User does not belong to this game"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.SelectQuestion(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.SelectQuestion(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task SelectQuestion_NotUserTurn_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-123",
            UserId = 2, // Not current turn player
            QuestionId = 100
        };

        _gameFlowServiceMock.Setup(x => x.SelectQuestion(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("It is not your turn"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.SelectQuestion(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.SelectQuestion(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task SelectQuestion_PreviousQuestionNotFinished_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-123",
            UserId = 1,
            QuestionId = 100
        };

        _gameFlowServiceMock.Setup(x => x.SelectQuestion(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("Previous question has not been answered"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.SelectQuestion(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.SelectQuestion(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task SelectQuestion_QuestionNotInGame_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-123",
            UserId = 1,
            QuestionId = 999 // Question not in this game
        };

        _gameFlowServiceMock.Setup(x => x.SelectQuestion(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("Question does not belong to this game"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.SelectQuestion(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.SelectQuestion(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task SelectQuestion_MissingQuestionId_ThrowsNoNullAllowedException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-123",
            UserId = 1,
            QuestionId = null // Missing QuestionId
        };

        _gameFlowServiceMock.Setup(x => x.SelectQuestion(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new NoNullAllowedException("QuestionId must be provided"));

        // Act & Assert
        await Assert.ThrowsAsync<NoNullAllowedException>(() =>
            _controller.SelectQuestion(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.SelectQuestion(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task SelectQuestion_MissingUserId_ThrowsNoNullAllowedException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-123",
            UserId = null, // Missing UserId
            QuestionId = 100
        };

        _gameFlowServiceMock.Setup(x => x.SelectQuestion(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new NoNullAllowedException("UserId must be provided"));

        // Act & Assert
        await Assert.ThrowsAsync<NoNullAllowedException>(() =>
            _controller.SelectQuestion(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.SelectQuestion(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task SelectQuestion_IdentityTheft_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-123",
            UserId = 5, // Different from authenticated user
            QuestionId = 100
        };

        _gameFlowServiceMock.Setup(x => x.SelectQuestion(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("Identity theft is not a joke"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.SelectQuestion(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.SelectQuestion(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task SelectQuestion_MultiplePlayersSelectingConcurrently_OnlyOneSucceeds()
    {
        // Arrange
        var gameFlowDto1 = new GameFlowDto
        {
            SessionId = "CONCURRENT-GAME",
            UserId = 1,
            QuestionId = 100
        };

        var gameFlowDto2 = new GameFlowDto
        {
            SessionId = "CONCURRENT-GAME",
            UserId = 2,
            QuestionId = 101
        };

        // First selection succeeds
        _gameFlowServiceMock.Setup(x => x.SelectQuestion(It.Is<GameFlowDto>(g => g.UserId == 1)))
            .Returns(Task.CompletedTask);

        // Second selection fails because it's not their turn
        _gameFlowServiceMock.Setup(x => x.SelectQuestion(It.Is<GameFlowDto>(g => g.UserId == 2)))
            .ThrowsAsync(new BusinessValidationException("It is not your turn"));

        // Act
        var result1 = await _controller.SelectQuestion(gameFlowDto1);

        // Assert
        Assert.IsType<OkResult>(result1);

        // Act & Assert for second user
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.SelectQuestion(gameFlowDto2));

        _gameFlowServiceMock.Verify(x => x.SelectQuestion(It.IsAny<GameFlowDto>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SelectQuestion_ValidSelectionAfterPreviousQuestionAnswered_ReturnsOk()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "SEQUENTIAL-GAME",
            UserId = 1,
            QuestionId = 200
        };

        _gameFlowServiceMock.Setup(x => x.SelectQuestion(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.SelectQuestion(gameFlowDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _gameFlowServiceMock.Verify(x => x.SelectQuestion(It.Is<GameFlowDto>(g =>
            g.SessionId == "SEQUENTIAL-GAME" &&
            g.UserId == 1 &&
            g.QuestionId == 200)), Times.Once);
    }
}