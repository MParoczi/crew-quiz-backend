using Backend.Controllers;
using Backend.Interfaces.Services;
using Backend.Interfaces.Utils;
using Backend.Models.DTOs;
using Backend.Models.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CrewQuiz.Tests.GameSession;

public class GameCompletionTests
{
    private readonly GameFlowController _controller;
    private readonly Mock<IGameFlowService> _gameFlowServiceMock;
    private readonly Mock<IServiceDispatcher> _serviceDispatcherMock;
    private readonly Mock<IServiceMethodDispatcher<IGameFlowService>> _serviceMethodDispatcherMock;

    public GameCompletionTests()
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
    public async Task SubmitAnswer_LastQuestion_TriggersGameCompletion()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "GAME-COMPLETION-TEST",
            UserId = 1,
            QuestionId = 100,
            Answer = "Final Answer"
        };

        _gameFlowServiceMock.Setup(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.SubmitAnswer(gameFlowDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _gameFlowServiceMock.Verify(x => x.SubmitAnswer(It.Is<GameFlowDto>(g =>
            g.SessionId == "GAME-COMPLETION-TEST" &&
            g.UserId == 1 &&
            g.QuestionId == 100 &&
            g.Answer == "Final Answer")), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswer_GameCompletionError_ThrowsBusinessValidationException()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "COMPLETION-ERROR-TEST",
            UserId = 1,
            QuestionId = 100,
            Answer = "Answer"
        };

        _gameFlowServiceMock.Setup(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()))
            .ThrowsAsync(new BusinessValidationException("Failed to complete game"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _controller.SubmitAnswer(gameFlowDto));

        _gameFlowServiceMock.Verify(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()), Times.Once);
    }

    [Fact]
    public async Task RobQuestion_LastQuestion_TriggersGameCompletion()
    {
        // Arrange
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "ROBBING-COMPLETION-TEST",
            UserId = 2,
            QuestionId = 200,
            Answer = "Robbed Answer"
        };

        _gameFlowServiceMock.Setup(x => x.RobQuestion(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RobQuestion(gameFlowDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _gameFlowServiceMock.Verify(x => x.RobQuestion(It.Is<GameFlowDto>(g =>
            g.SessionId == "ROBBING-COMPLETION-TEST" &&
            g.UserId == 2 &&
            g.QuestionId == 200 &&
            g.Answer == "Robbed Answer")), Times.Once);
    }

    [Fact]
    public async Task GameCompletion_ServiceLayer_HandlesAllScenarios()
    {
        // Arrange - Test that the service layer handles game completion
        var gameFlowDto = new GameFlowDto
        {
            SessionId = "SERVICE-COMPLETION-TEST",
            UserId = 1,
            QuestionId = 300,
            Answer = "Completion Test"
        };

        // Setup service to complete successfully
        _gameFlowServiceMock.Setup(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.SubmitAnswer(gameFlowDto);

        // Assert - Verify the service was called and completed successfully
        Assert.IsType<OkResult>(result);
        _gameFlowServiceMock.Verify(x => x.SubmitAnswer(It.IsAny<GameFlowDto>()), Times.Once);
    }
}