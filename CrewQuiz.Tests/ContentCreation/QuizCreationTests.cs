using System.Security.Claims;
using Backend.Controllers;
using Backend.Interfaces.Services;
using Backend.Interfaces.Utils;
using Backend.Models.DTOs;
using Backend.Models.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CrewQuiz.Tests.ContentCreation;

/// <summary>
///     Tests for Story 4: Quiz Creation
///     Tests all CRUD operations for Quiz management
/// </summary>
public class QuizCreationTests : TestBase
{
    private readonly QuizController _controller;
    private readonly Mock<IQuizService> _quizServiceMock;
    private readonly Mock<IServiceDispatcher> _serviceDispatcherMock;
    private readonly Mock<IServiceMethodDispatcher<IQuizService>> _serviceMethodDispatcherMock;

    public QuizCreationTests()
    {
        _serviceDispatcherMock = new Mock<IServiceDispatcher>();
        _serviceMethodDispatcherMock = new Mock<IServiceMethodDispatcher<IQuizService>>();
        _quizServiceMock = new Mock<IQuizService>();

        // Setup service dispatcher to return service method dispatcher mock
        _serviceDispatcherMock.Setup(x => x.For<IQuizService>())
            .Returns(_serviceMethodDispatcherMock.Object);

        // Setup service method dispatcher to call the actual service methods
        _serviceMethodDispatcherMock.Setup(x => x.DispatchAsync(It.IsAny<Func<IQuizService, Task>>()))
            .Returns((Func<IQuizService, Task> method) => method(_quizServiceMock.Object));

        _serviceMethodDispatcherMock.Setup(x => x.DispatchAsync(It.IsAny<Func<IQuizService, Task<IEnumerable<QuizDto>>>>()))
            .Returns((Func<IQuizService, Task<IEnumerable<QuizDto>>> method) => method(_quizServiceMock.Object));

        _serviceMethodDispatcherMock.Setup(x => x.DispatchAsync(It.IsAny<Func<IQuizService, Task<QuizDto>>>()))
            .Returns((Func<IQuizService, Task<QuizDto>> method) => method(_quizServiceMock.Object));

        _controller = new QuizController(_serviceDispatcherMock.Object);

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
    public async Task CreateQuiz_ValidQuizDto_ReturnsOk()
    {
        // Arrange
        var quizDto = new QuizDto
        {
            QuizId = 0,
            Name = "Test Quiz",
            QuestionGroups = []
        };

        _quizServiceMock.Setup(x => x.CreateQuiz(It.IsAny<QuizDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateQuiz(quizDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _quizServiceMock.Verify(x => x.CreateQuiz(It.Is<QuizDto>(q => q.Name == "Test Quiz")), Times.Once);
        Console.WriteLine("[DEBUG_LOG] CreateQuiz test passed - Quiz created successfully");
    }

    [Fact]
    public async Task GetQuizzesForCurrentUser_ValidRequest_ReturnsQuizzes()
    {
        // Arrange
        var expectedQuizzes = new List<QuizDto>
        {
            new() { QuizId = 1, Name = "Quiz 1", QuestionGroups = [] },
            new() { QuizId = 2, Name = "Quiz 2", QuestionGroups = [] }
        };

        _quizServiceMock.Setup(x => x.GetQuizzesForCurrentUser())
            .ReturnsAsync(expectedQuizzes);

        // Act
        var result = await _controller.GetQuizzesForCurrentUser();

        // Assert
        var okResult = Assert.IsType<ActionResult<IEnumerable<QuizDto>>>(result);
        var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var quizzes = Assert.IsAssignableFrom<IEnumerable<QuizDto>>(okObjectResult.Value);
        Assert.Equal(2, quizzes.Count());
        Console.WriteLine("[DEBUG_LOG] GetQuizzesForCurrentUser test passed - Retrieved user's quizzes");
    }

    [Fact]
    public async Task GetQuiz_ValidQuizId_ReturnsQuiz()
    {
        // Arrange
        const long quizId = 1;
        var expectedQuiz = new QuizDto { QuizId = quizId, Name = "Test Quiz", QuestionGroups = [] };

        _quizServiceMock.Setup(x => x.GetQuiz(quizId))
            .ReturnsAsync(expectedQuiz);

        // Act
        var result = await _controller.GetQuiz(quizId);

        // Assert
        var okResult = Assert.IsType<ActionResult<QuizDto>>(result);
        var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var quiz = Assert.IsType<QuizDto>(okObjectResult.Value);
        Assert.Equal(quizId, quiz.QuizId);
        Assert.Equal("Test Quiz", quiz.Name);
        Console.WriteLine("[DEBUG_LOG] GetQuiz test passed - Retrieved specific quiz");
    }

    [Fact]
    public async Task GetQuiz_InvalidQuizId_ThrowsBusinessValidationException()
    {
        // Arrange
        const long invalidQuizId = 999;

        _quizServiceMock.Setup(x => x.GetQuiz(invalidQuizId))
            .ThrowsAsync(new BusinessValidationException("Quiz was not found"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() => _controller.GetQuiz(invalidQuizId));
        Console.WriteLine("[DEBUG_LOG] GetQuiz with invalid ID test passed - Exception thrown correctly");
    }

    [Fact]
    public async Task UpdateQuiz_ValidQuizDto_ReturnsOk()
    {
        // Arrange
        var quizDto = new QuizDto
        {
            QuizId = 1,
            Name = "Updated Quiz Name",
            QuestionGroups = []
        };

        _quizServiceMock.Setup(x => x.UpdateQuiz(It.IsAny<QuizDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateQuiz(quizDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _quizServiceMock.Verify(x => x.UpdateQuiz(It.Is<QuizDto>(q => q.QuizId == 1 && q.Name == "Updated Quiz Name")), Times.Once);
        Console.WriteLine("[DEBUG_LOG] UpdateQuiz test passed - Quiz updated successfully");
    }

    [Fact]
    public async Task UpdateQuiz_UnauthorizedUser_ThrowsBusinessValidationException()
    {
        // Arrange
        var quizDto = new QuizDto
        {
            QuizId = 1,
            Name = "Updated Quiz Name",
            QuestionGroups = []
        };

        _quizServiceMock.Setup(x => x.UpdateQuiz(It.IsAny<QuizDto>()))
            .ThrowsAsync(new BusinessValidationException("Quiz cannot be updated by someone else"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() => _controller.UpdateQuiz(quizDto));
        Console.WriteLine("[DEBUG_LOG] UpdateQuiz unauthorized test passed - Exception thrown correctly");
    }

    [Fact]
    public async Task DeleteQuiz_ValidQuizId_ReturnsOk()
    {
        // Arrange
        const long quizId = 1;

        _quizServiceMock.Setup(x => x.DeleteQuiz(quizId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteQuiz(quizId);

        // Assert
        Assert.IsType<OkResult>(result);
        _quizServiceMock.Verify(x => x.DeleteQuiz(quizId), Times.Once);
        Console.WriteLine("[DEBUG_LOG] DeleteQuiz test passed - Quiz deleted successfully");
    }

    [Fact]
    public async Task DeleteQuiz_UnauthorizedUser_ThrowsBusinessValidationException()
    {
        // Arrange
        const long quizId = 1;

        _quizServiceMock.Setup(x => x.DeleteQuiz(quizId))
            .ThrowsAsync(new BusinessValidationException("Quiz cannot be deleted by someone else"));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() => _controller.DeleteQuiz(quizId));
        Console.WriteLine("[DEBUG_LOG] DeleteQuiz unauthorized test passed - Exception thrown correctly");
    }

    [Fact]
    public async Task GetQuizByCurrentGameId_ValidGameId_ReturnsQuiz()
    {
        // Arrange
        const long gameId = 1;
        var expectedQuiz = new QuizDto { QuizId = 1, Name = "Game Quiz", QuestionGroups = [] };

        _quizServiceMock.Setup(x => x.GetQuizByCurrentGameId(gameId))
            .ReturnsAsync(expectedQuiz);

        // Act
        var result = await _controller.GetQuizByCurrentGameId(gameId);

        // Assert
        var okResult = Assert.IsType<ActionResult<QuizDto>>(result);
        var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var quiz = Assert.IsType<QuizDto>(okObjectResult.Value);
        Assert.Equal("Game Quiz", quiz.Name);
        Console.WriteLine("[DEBUG_LOG] GetQuizByCurrentGameId test passed - Retrieved quiz by game ID");
    }
}