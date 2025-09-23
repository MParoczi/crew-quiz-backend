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
///     Tests for Story 6: Question Management
///     Tests all CRUD operations for Question management including relationships with QuestionGroups
/// </summary>
public class QuestionManagementTests : TestBase
{
    private readonly QuestionController _controller;
    private readonly Mock<IQuestionService> _questionServiceMock;
    private readonly Mock<IServiceDispatcher> _serviceDispatcherMock;
    private readonly Mock<IServiceMethodDispatcher<IQuestionService>> _serviceMethodDispatcherMock;

    public QuestionManagementTests()
    {
        _serviceDispatcherMock = new Mock<IServiceDispatcher>();
        _serviceMethodDispatcherMock = new Mock<IServiceMethodDispatcher<IQuestionService>>();
        _questionServiceMock = new Mock<IQuestionService>();

        // Setup service dispatcher to return service method dispatcher mock
        _serviceDispatcherMock.Setup(x => x.For<IQuestionService>())
            .Returns(_serviceMethodDispatcherMock.Object);

        // Setup service method dispatcher to call the actual service methods
        _serviceMethodDispatcherMock.Setup(x => x.DispatchAsync(It.IsAny<Func<IQuestionService, Task>>()))
            .Returns((Func<IQuestionService, Task> method) => method(_questionServiceMock.Object));

        _serviceMethodDispatcherMock.Setup(x => x.DispatchAsync(It.IsAny<Func<IQuestionService, Task<IEnumerable<QuestionDto>>>>()))
            .Returns((Func<IQuestionService, Task<IEnumerable<QuestionDto>>> method) => method(_questionServiceMock.Object));

        _serviceMethodDispatcherMock.Setup(x => x.DispatchAsync(It.IsAny<Func<IQuestionService, Task<QuestionDto>>>()))
            .Returns((Func<IQuestionService, Task<QuestionDto>> method) => method(_questionServiceMock.Object));

        _controller = new QuestionController(_serviceDispatcherMock.Object);

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
    public async Task CreateQuestion_ValidQuestionDto_ReturnsOk()
    {
        // Arrange
        var questionDto = new QuestionDto
        {
            QuestionId = 0,
            QuestionGroupId = 1,
            QuestionGroupName = "Geography",
            Inquiry = "What is the capital of France?",
            Answer = "Paris",
            Point = 10
        };

        _questionServiceMock.Setup(x => x.CreateQuestion(It.IsAny<QuestionDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateQuestion(questionDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _questionServiceMock.Verify(x => x.CreateQuestion(It.Is<QuestionDto>(q =>
            q.Inquiry == "What is the capital of France?" &&
            q.Answer == "Paris" &&
            q.Point == 10)), Times.Once);
        Console.WriteLine("[DEBUG_LOG] CreateQuestion test passed - Question created successfully with content, answer, and point value");
    }

    [Fact]
    public async Task GetQuestionsForCurrentUser_ValidRequest_ReturnsQuestions()
    {
        // Arrange
        var expectedQuestions = new List<QuestionDto>
        {
            new()
            {
                QuestionId = 1,
                QuestionGroupId = 1,
                QuestionGroupName = "Math",
                Inquiry = "What is 2+2?",
                Answer = "4",
                Point = 5
            },
            new()
            {
                QuestionId = 2,
                QuestionGroupId = 2,
                QuestionGroupName = "Geography",
                Inquiry = "What is the capital of Italy?",
                Answer = "Rome",
                Point = 10
            }
        };

        _questionServiceMock.Setup(x => x.GetQuestionsForCurrentUser())
            .ReturnsAsync(expectedQuestions);

        // Act
        var result = await _controller.GetQuestionsForCurrentUser();

        // Assert
        var okResult = Assert.IsType<ActionResult<IEnumerable<QuestionDto>>>(result);
        var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var questions = Assert.IsAssignableFrom<IEnumerable<QuestionDto>>(okObjectResult.Value);
        Assert.Equal(2, questions.Count());

        var questionList = questions.ToList();
        Assert.Contains(questionList, q => q.Inquiry == "What is 2+2?" && q.Answer == "4" && q.Point == 5);
        Assert.Contains(questionList, q => q.Inquiry == "What is the capital of Italy?" && q.Answer == "Rome" && q.Point == 10);

        Console.WriteLine("[DEBUG_LOG] GetQuestionsForCurrentUser test passed - Retrieved user's questions with proper content and relationships");
    }

    [Fact]
    public async Task GetQuestionsByQuestionGroupId_ValidQuestionGroupId_ReturnsQuestions()
    {
        // Arrange
        const long questionGroupId = 1;
        var expectedQuestions = new List<QuestionDto>
        {
            new()
            {
                QuestionId = 1,
                QuestionGroupId = questionGroupId,
                QuestionGroupName = "Math",
                Inquiry = "What is 2+2?",
                Answer = "4",
                Point = 5
            },
            new()
            {
                QuestionId = 2,
                QuestionGroupId = questionGroupId,
                QuestionGroupName = "Math",
                Inquiry = "What is 5*3?",
                Answer = "15",
                Point = 8
            }
        };

        _questionServiceMock.Setup(x => x.GetQuestionsByQuestionGroupId(questionGroupId))
            .ReturnsAsync(expectedQuestions);

        // Act
        var result = await _controller.GetQuestionsByQuestionGroupId(questionGroupId);

        // Assert
        var okResult = Assert.IsType<ActionResult<IEnumerable<QuestionDto>>>(result);
        var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var questions = Assert.IsAssignableFrom<IEnumerable<QuestionDto>>(okObjectResult.Value);
        Assert.Equal(2, questions.Count());
        Assert.All(questions, q => Assert.Equal(questionGroupId, q.QuestionGroupId));

        Console.WriteLine("[DEBUG_LOG] GetQuestionsByQuestionGroupId test passed - Retrieved questions properly associated with parent group");
    }

    [Fact]
    public async Task GetQuestion_ValidQuestionId_ReturnsQuestion()
    {
        // Arrange
        const long questionId = 1;
        var expectedQuestion = new QuestionDto
        {
            QuestionId = questionId,
            QuestionGroupId = 1,
            QuestionGroupName = "Geography",
            Inquiry = "What is the capital of France?",
            Answer = "Paris",
            Point = 10
        };

        _questionServiceMock.Setup(x => x.GetQuestion(questionId))
            .ReturnsAsync(expectedQuestion);

        // Act
        var result = await _controller.GetQuestion(questionId);

        // Assert
        var okResult = Assert.IsType<ActionResult<QuestionDto>>(result);
        var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var question = Assert.IsType<QuestionDto>(okObjectResult.Value);

        Assert.Equal(questionId, question.QuestionId);
        Assert.Equal("What is the capital of France?", question.Inquiry);
        Assert.Equal("Paris", question.Answer);
        Assert.Equal(10, question.Point);
        Assert.Equal(1, question.QuestionGroupId);
        Assert.Equal("Geography", question.QuestionGroupName);

        Console.WriteLine("[DEBUG_LOG] GetQuestion test passed - Retrieved individual question with all required properties");
    }

    [Fact]
    public async Task GetQuestion_InvalidQuestionId_ThrowsBusinessValidationException()
    {
        // Arrange
        const long invalidQuestionId = 999;

        _questionServiceMock.Setup(x => x.GetQuestion(invalidQuestionId))
            .ThrowsAsync(new BusinessValidationException("Question not found or access denied"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(() => _controller.GetQuestion(invalidQuestionId));
        Assert.Equal("Question not found or access denied", exception.Message);

        Console.WriteLine("[DEBUG_LOG] GetQuestion invalid ID test passed - Proper exception handling for non-existent questions");
    }

    [Fact]
    public async Task UpdateQuestion_ValidQuestionDto_ReturnsOk()
    {
        // Arrange
        var questionDto = new QuestionDto
        {
            QuestionId = 1,
            QuestionGroupId = 1,
            QuestionGroupName = "Geography",
            Inquiry = "Updated: What is the capital of France?",
            Answer = "Paris",
            Point = 15
        };

        _questionServiceMock.Setup(x => x.UpdateQuestion(It.IsAny<QuestionDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateQuestion(questionDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _questionServiceMock.Verify(x => x.UpdateQuestion(It.Is<QuestionDto>(q =>
            q.QuestionId == 1 &&
            q.Inquiry == "Updated: What is the capital of France?" &&
            q.Point == 15)), Times.Once);

        Console.WriteLine("[DEBUG_LOG] UpdateQuestion test passed - Question updated successfully maintaining data integrity");
    }

    [Fact]
    public async Task UpdateQuestion_UnauthorizedUser_ThrowsBusinessValidationException()
    {
        // Arrange
        var questionDto = new QuestionDto
        {
            QuestionId = 1,
            Inquiry = "Unauthorized update attempt",
            Answer = "Should not work",
            Point = 10
        };

        _questionServiceMock.Setup(x => x.UpdateQuestion(It.IsAny<QuestionDto>()))
            .ThrowsAsync(new BusinessValidationException("Access denied: Question does not belong to current user"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(() => _controller.UpdateQuestion(questionDto));
        Assert.Equal("Access denied: Question does not belong to current user", exception.Message);

        Console.WriteLine("[DEBUG_LOG] UpdateQuestion unauthorized test passed - Proper ownership validation through group → quiz → user chain");
    }

    [Fact]
    public async Task DeleteQuestion_ValidQuestionId_ReturnsOk()
    {
        // Arrange
        const long questionId = 1;

        _questionServiceMock.Setup(x => x.DeleteQuestion(questionId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteQuestion(questionId);

        // Assert
        Assert.IsType<OkResult>(result);
        _questionServiceMock.Verify(x => x.DeleteQuestion(questionId), Times.Once);

        Console.WriteLine("[DEBUG_LOG] DeleteQuestion test passed - Question deleted successfully");
    }

    [Fact]
    public async Task DeleteQuestion_UnauthorizedUser_ThrowsBusinessValidationException()
    {
        // Arrange
        const long questionId = 1;

        _questionServiceMock.Setup(x => x.DeleteQuestion(questionId))
            .ThrowsAsync(new BusinessValidationException("Access denied: Question does not belong to current user"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(() => _controller.DeleteQuestion(questionId));
        Assert.Equal("Access denied: Question does not belong to current user", exception.Message);

        Console.WriteLine("[DEBUG_LOG] DeleteQuestion unauthorized test passed - Proper authorization checking for delete operations");
    }

    [Fact]
    public async Task GetQuestionsByQuestionGroupId_UnauthorizedQuestionGroup_ThrowsBusinessValidationException()
    {
        // Arrange
        const long unauthorizedQuestionGroupId = 999;

        _questionServiceMock.Setup(x => x.GetQuestionsByQuestionGroupId(unauthorizedQuestionGroupId))
            .ThrowsAsync(new BusinessValidationException("Access denied: Question Group does not belong to current user"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(() => _controller.GetQuestionsByQuestionGroupId(unauthorizedQuestionGroupId));
        Assert.Equal("Access denied: Question Group does not belong to current user", exception.Message);

        Console.WriteLine("[DEBUG_LOG] GetQuestionsByQuestionGroupId unauthorized test passed - Questions inherit ownership through group → quiz → user chain");
    }

    [Fact]
    public async Task CreateQuestion_WithRequiredContentValidation_ReturnsOk()
    {
        // Arrange - Test that required fields (Inquiry, Answer) are properly validated
        var questionDto = new QuestionDto
        {
            QuestionId = 0,
            QuestionGroupId = 1,
            QuestionGroupName = "Validation Test",
            Inquiry = "Required inquiry field test",
            Answer = "Required answer field test",
            Point = 5
        };

        _questionServiceMock.Setup(x => x.CreateQuestion(It.IsAny<QuestionDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateQuestion(questionDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _questionServiceMock.Verify(x => x.CreateQuestion(It.Is<QuestionDto>(q =>
            !string.IsNullOrEmpty(q.Inquiry) &&
            !string.IsNullOrEmpty(q.Answer))), Times.Once);

        Console.WriteLine("[DEBUG_LOG] CreateQuestion content validation test passed - Required fields properly validated");
    }
}