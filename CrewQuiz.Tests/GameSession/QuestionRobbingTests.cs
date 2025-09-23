using System.Data;
using Backend.Controllers;
using Backend.Interfaces.Services;
using Backend.Interfaces.Utils;
using Backend.Models.DTOs;
using Backend.Models.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CrewQuiz.Tests.GameSession;

public class QuestionRobbingTests
{
    private readonly GameFlowController _controller;
    private readonly Mock<IGameFlowService> _gameFlowServiceMock;
    private readonly Mock<IServiceDispatcher> _serviceDispatcherMock;
    private readonly Mock<IServiceMethodDispatcher<IGameFlowService>> _serviceMethodDispatcherMock;

    public QuestionRobbingTests()
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
    public async Task RobQuestion_ValidRequest_ReturnsOk()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-ROB-123",
            UserId = 2, // Different user robbing the question
            QuestionId = 100,
            Answer = "Correct Answer"
        };

        _gameFlowServiceMock.Setup(x => x.RobQuestion(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RobQuestion(gameFlowDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _gameFlowServiceMock.Verify(x => x.RobQuestion(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task RobQuestion_MissingSessionId_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "", // Missing SessionId
            UserId = 2,
            QuestionId = 100,
            Answer = "Test Answer"
        };

        _gameFlowServiceMock.Setup(x => x.RobQuestion(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("Game was not found"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.RobQuestion(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.RobQuestion(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task RobQuestion_MissingQuestionId_ThrowsNoNullAllowedException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-ROB-123",
            UserId = 2,
            QuestionId = null, // Missing QuestionId
            Answer = "Test Answer"
        };

        _gameFlowServiceMock.Setup(x => x.RobQuestion(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new NoNullAllowedException("QuestionId must be provided"));

        // Act & Assert
        await Assert.ThrowsAsync<NoNullAllowedException>(() =>
            _controller.RobQuestion(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.RobQuestion(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task RobQuestion_MissingUserId_ThrowsNoNullAllowedException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-ROB-123",
            UserId = null, // Missing UserId
            QuestionId = 100,
            Answer = "Test Answer"
        };

        _gameFlowServiceMock.Setup(x => x.RobQuestion(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new NoNullAllowedException("UserId must be provided"));

        // Act & Assert
        await Assert.ThrowsAsync<NoNullAllowedException>(() =>
            _controller.RobQuestion(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.RobQuestion(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task RobQuestion_MissingAnswer_ThrowsNoNullAllowedException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-ROB-123",
            UserId = 2,
            QuestionId = 100,
            Answer = null // Missing Answer
        };

        _gameFlowServiceMock.Setup(x => x.RobQuestion(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new NoNullAllowedException("Answer must be provided"));

        // Act & Assert
        await Assert.ThrowsAsync<NoNullAllowedException>(() =>
            _controller.RobQuestion(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.RobQuestion(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task RobQuestion_QuestionNotAvailableForRobbing_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-ROB-123",
            UserId = 2,
            QuestionId = 100,
            Answer = "Test Answer"
        };

        _gameFlowServiceMock.Setup(x => x.RobQuestion(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("Question is not available for robbing"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.RobQuestion(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.RobQuestion(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task RobQuestion_QuestionAlreadyAnswered_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-ROB-123",
            UserId = 2,
            QuestionId = 100,
            Answer = "Test Answer"
        };

        _gameFlowServiceMock.Setup(x => x.RobQuestion(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("Question has already been answered"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.RobQuestion(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.RobQuestion(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task RobQuestion_UserNotInGame_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-ROB-123",
            UserId = 999, // User not in the game
            QuestionId = 100,
            Answer = "Test Answer"
        };

        _gameFlowServiceMock.Setup(x => x.RobQuestion(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("User does not belong to this game"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.RobQuestion(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.RobQuestion(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task RobQuestion_IdentityTheft_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-ROB-123",
            UserId = 1, // UserId doesn't match authenticated user
            QuestionId = 100,
            Answer = "Test Answer"
        };

        _gameFlowServiceMock.Setup(x => x.RobQuestion(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("Identity theft is not a joke"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.RobQuestion(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.RobQuestion(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task RobQuestion_ConcurrentRobbingAttempts_UsesThreadSafety()
    {
        // Arrange
        var gameFlowDto1 = new GameFlowDto
        {
            SessionId = "TEST-SESSION-CONCURRENT-ROB",
            UserId = 2,
            QuestionId = 100,
            Answer = "Answer 1"
        };

        var gameFlowDto2 = new GameFlowDto
        {
            SessionId = "TEST-SESSION-CONCURRENT-ROB", // Same session
            UserId = 3,
            QuestionId = 100,
            Answer = "Answer 2"
        };

        _gameFlowServiceMock.Setup(x => x.RobQuestion(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var task1 = _controller.RobQuestion(gameFlowDto1);
        var task2 = _controller.RobQuestion(gameFlowDto2);

        await Task.WhenAll(task1, task2);

        // Assert
        Assert.IsType<OkResult>(await task1);
        Assert.IsType<OkResult>(await task2);
        _gameFlowServiceMock.Verify(x => x.RobQuestion(It.IsAny<GameFlowDto>()), Times.Exactly(2));
    }

    [Fact]
    public async Task RobQuestion_SuccessfulRobbingScenario_ProcessedCorrectly()
    {
        // Arrange - Simulating a scenario where user 2 robs a question that user 1 got wrong
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-SUCCESSFUL-ROB",
            UserId = 2, // User attempting to rob
            QuestionId = 100,
            Answer = "Correct Answer"
        };

        _gameFlowServiceMock.Setup(x => x.RobQuestion(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RobQuestion(gameFlowDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _gameFlowServiceMock.Verify(x => x.RobQuestion(gameFlowDto), Times.Once);
    }

    [Fact]
    public async Task RobQuestion_IncorrectRobbingAttempt_KeepsQuestionAvailableForRobbing()
    {
        // Arrange - User provides wrong answer while attempting to rob
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-INCORRECT-ROB",
            UserId = 2,
            QuestionId = 100,
            Answer = "Wrong Answer"
        };

        _gameFlowServiceMock.Setup(x => x.RobQuestion(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask); // Service will handle keeping question available

        // Act
        var result = await _controller.RobQuestion(gameFlowDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _gameFlowServiceMock.Verify(x => x.RobQuestion(It.IsAny<GameFlowDto>()), Times.Once);
    }
}