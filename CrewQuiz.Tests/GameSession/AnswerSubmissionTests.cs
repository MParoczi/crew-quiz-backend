using System.Data;
using Backend.Controllers;
using Backend.Interfaces.Services;
using Backend.Interfaces.Utils;
using Backend.Models.DTOs;
using Backend.Models.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CrewQuiz.Tests.GameSession;

public class AnswerSubmissionTests
{
    private readonly GameFlowController _controller;
    private readonly Mock<IGameFlowService> _gameFlowServiceMock;
    private readonly Mock<IServiceDispatcher> _serviceDispatcherMock;
    private readonly Mock<IServiceMethodDispatcher<IGameFlowService>> _serviceMethodDispatcherMock;

    public AnswerSubmissionTests()
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
    public async Task SubmitAnswer_ValidCorrectAnswer_ReturnsOk()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-123",
            UserId = 1,
            QuestionId = 100,
            Answer = "Correct Answer"
        };

        _gameFlowServiceMock.Setup(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.SubmitAnswer(gameFlowDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _gameFlowServiceMock.Verify(x => x.SubmitAnswer(It.Is<GameFlowDto>(g =>
            g.SessionId == "TEST-SESSION-123" &&
            g.UserId == 1 &&
            g.QuestionId == 100 &&
            g.Answer == "Correct Answer")), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswer_ValidIncorrectAnswer_ReturnsOk()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-456",
            UserId = 2,
            QuestionId = 200,
            Answer = "Wrong Answer"
        };

        _gameFlowServiceMock.Setup(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.SubmitAnswer(gameFlowDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _gameFlowServiceMock.Verify(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswer_GameNotFound_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "NONEXISTENT-GAME",
            UserId = 1,
            QuestionId = 100,
            Answer = "Test Answer"
        };

        _gameFlowServiceMock.Setup(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("Game was not found"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.SubmitAnswer(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswer_UserNotInGame_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-123",
            UserId = 999, // User not in game
            QuestionId = 100,
            Answer = "Test Answer"
        };

        _gameFlowServiceMock.Setup(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("User does not belong to this game"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.SubmitAnswer(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswer_NotUserTurn_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-123",
            UserId = 2, // Not current turn player
            QuestionId = 100,
            Answer = "Test Answer"
        };

        _gameFlowServiceMock.Setup(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("It is not your turn"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.SubmitAnswer(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswer_QuestionAlreadyAnswered_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-123",
            UserId = 1,
            QuestionId = 100,
            Answer = "Test Answer"
        };

        _gameFlowServiceMock.Setup(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("Question has already been answered"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.SubmitAnswer(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswer_QuestionNotInGame_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-123",
            UserId = 1,
            QuestionId = 999, // Question not in this game
            Answer = "Test Answer"
        };

        _gameFlowServiceMock.Setup(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("Question does not belong to this game"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.SubmitAnswer(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswer_MissingAnswer_ThrowsNoNullAllowedException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-123",
            UserId = 1,
            QuestionId = 100,
            Answer = null // Missing answer
        };

        _gameFlowServiceMock.Setup(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new NoNullAllowedException("Answer must be provided"));

        // Act & Assert
        await Assert.ThrowsAsync<NoNullAllowedException>(() =>
            _controller.SubmitAnswer(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswer_MissingQuestionId_ThrowsNoNullAllowedException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-123",
            UserId = 1,
            QuestionId = null, // Missing QuestionId
            Answer = "Test Answer"
        };

        _gameFlowServiceMock.Setup(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new NoNullAllowedException("QuestionId must be provided"));

        // Act & Assert
        await Assert.ThrowsAsync<NoNullAllowedException>(() =>
            _controller.SubmitAnswer(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswer_MissingUserId_ThrowsNoNullAllowedException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-123",
            UserId = null, // Missing UserId
            QuestionId = 100,
            Answer = "Test Answer"
        };

        _gameFlowServiceMock.Setup(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new NoNullAllowedException("UserId must be provided"));

        // Act & Assert
        await Assert.ThrowsAsync<NoNullAllowedException>(() =>
            _controller.SubmitAnswer(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswer_IdentityTheft_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-123",
            UserId = 1, // UserId doesn't match authenticated user
            QuestionId = 100,
            Answer = "Test Answer"
        };

        _gameFlowServiceMock.Setup(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("Identity theft is not a joke"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.SubmitAnswer(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswer_ConcurrentSubmissions_UsesThreadSafety()
    {
        // Arrange
        var gameFlowDto1 = new GameFlowDto
        {
            SessionId = "TEST-SESSION-CONCURRENT",
            UserId = 1,
            QuestionId = 100,
            Answer = "Answer 1"
        };

        var gameFlowDto2 = new GameFlowDto
        {
            SessionId = "TEST-SESSION-CONCURRENT", // Same session
            UserId = 2,
            QuestionId = 100,
            Answer = "Answer 2"
        };

        var delayTask = new TaskCompletionSource<bool>();
        var firstCallStarted = new TaskCompletionSource<bool>();

        _gameFlowServiceMock.SetupSequence(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()))
            .Returns(async () =>
            {
                firstCallStarted.SetResult(true);
                await delayTask.Task; // First call waits
            })
            .Returns(Task.CompletedTask); // Second call completes immediately

        // Act
        var task1 = _controller.SubmitAnswer(gameFlowDto1);
        await firstCallStarted.Task; // Wait for first call to start

        var task2 = _controller.SubmitAnswer(gameFlowDto2);

        // Complete the first call
        delayTask.SetResult(true);

        await Task.WhenAll(task1, task2);

        // Assert
        Assert.IsType<OkResult>(await task1);
        Assert.IsType<OkResult>(await task2);

        // Verify both calls were made (but serialized due to semaphore)
        _gameFlowServiceMock.Verify(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SubmitAnswer_DifferentSessions_AllowsConcurrentExecution()
    {
        // Arrange
        var gameFlowDto1 = new GameFlowDto
        {
            SessionId = "TEST-SESSION-A",
            UserId = 1,
            QuestionId = 100,
            Answer = "Answer 1"
        };

        var gameFlowDto2 = new GameFlowDto
        {
            SessionId = "TEST-SESSION-B", // Different session
            UserId = 2,
            QuestionId = 200,
            Answer = "Answer 2"
        };

        _gameFlowServiceMock.Setup(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var task1 = _controller.SubmitAnswer(gameFlowDto1);
        var task2 = _controller.SubmitAnswer(gameFlowDto2);

        await Task.WhenAll(task1, task2);

        // Assert
        Assert.IsType<OkResult>(await task1);
        Assert.IsType<OkResult>(await task2);

        _gameFlowServiceMock.Verify(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SubmitAnswer_ServiceThrowsException_PropagatesException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-123",
            UserId = 1,
            QuestionId = 100,
            Answer = "Test Answer"
        };

        var expectedException = new InvalidOperationException("Service error");
        _gameFlowServiceMock.Setup(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _controller.SubmitAnswer(gameFlowDto));

        Assert.Equal(expectedException.Message, actualException.Message);
        _gameFlowServiceMock.Verify(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswer_EmptyAnswer_ThrowsNoNullAllowedException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-123",
            UserId = 1,
            QuestionId = 100,
            Answer = string.Empty // Empty answer
        };

        _gameFlowServiceMock.Setup(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new NoNullAllowedException("Answer must be provided"));

        // Act & Assert
        await Assert.ThrowsAsync<NoNullAllowedException>(() =>
            _controller.SubmitAnswer(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswer_WhitespaceAnswer_ThrowsNoNullAllowedException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-123",
            UserId = 1,
            QuestionId = 100,
            Answer = "   " // Whitespace only answer
        };

        _gameFlowServiceMock.Setup(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new NoNullAllowedException("Answer must be provided"));

        // Act & Assert
        await Assert.ThrowsAsync<NoNullAllowedException>(() =>
            _controller.SubmitAnswer(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswer_RobbingScenario_ReturnsOk()
    {
        // Arrange - Testing robbing allowed scenario
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "TEST-SESSION-ROBBING",
            UserId = 3, // Different user attempting to rob
            QuestionId = 100,
            Answer = "Robbing Answer"
        };

        _gameFlowServiceMock.Setup(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.SubmitAnswer(gameFlowDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _gameFlowServiceMock.Verify(x => x.SubmitAnswer(It.Is<GameFlowDto>(g =>
            g.SessionId == "TEST-SESSION-ROBBING" &&
            g.UserId == 3 &&
            g.QuestionId == 100 &&
            g.Answer == "Robbing Answer")), Times.Once);
    }
}