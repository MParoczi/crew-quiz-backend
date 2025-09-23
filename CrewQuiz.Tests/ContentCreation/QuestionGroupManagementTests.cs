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
///     Tests for Story 5: Question Group Management
///     Tests all CRUD operations for QuestionGroup management including hierarchical relationships
/// </summary>
public class QuestionGroupManagementTests : TestBase
{
    private readonly QuestionGroupController _controller;
    private readonly Mock<IQuestionGroupService> _questionGroupServiceMock;
    private readonly Mock<IServiceDispatcher> _serviceDispatcherMock;
    private readonly Mock<IServiceMethodDispatcher<IQuestionGroupService>> _serviceMethodDispatcherMock;

    public QuestionGroupManagementTests()
    {
        _serviceDispatcherMock = new Mock<IServiceDispatcher>();
        _serviceMethodDispatcherMock = new Mock<IServiceMethodDispatcher<IQuestionGroupService>>();
        _questionGroupServiceMock = new Mock<IQuestionGroupService>();

        // Setup service dispatcher to return service method dispatcher mock
        _serviceDispatcherMock.Setup(x => x.For<IQuestionGroupService>())
            .Returns(_serviceMethodDispatcherMock.Object);

        // Setup service method dispatcher to call the actual service methods
        _serviceMethodDispatcherMock.Setup(x => x.DispatchAsync(It.IsAny<Func<IQuestionGroupService, Task>>()))
            .Returns((Func<IQuestionGroupService, Task> method) => method(_questionGroupServiceMock.Object));

        _serviceMethodDispatcherMock.Setup(x =>
                x.DispatchAsync(It.IsAny<Func<IQuestionGroupService, Task<IEnumerable<QuestionGroupDto>>>>()))
            .Returns((Func<IQuestionGroupService, Task<IEnumerable<QuestionGroupDto>>> method) => method(_questionGroupServiceMock.Object));

        _serviceMethodDispatcherMock.Setup(x => x.DispatchAsync(It.IsAny<Func<IQuestionGroupService, Task<QuestionGroupDto>>>()))
            .Returns((Func<IQuestionGroupService, Task<QuestionGroupDto>> method) => method(_questionGroupServiceMock.Object));

        _controller = new QuestionGroupController(_serviceDispatcherMock.Object);

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
    public async Task CreateQuestionGroup_ValidQuestionGroupDto_ReturnsOk()
    {
        // Arrange
        var questionGroupDto = new QuestionGroupDto
        {
            QuestionGroupId = 0,
            Name = "Test Question Group",
            Description = "A test question group for Story 5",
            Questions = []
        };

        _questionGroupServiceMock.Setup(x => x.CreateQuestionGroup(It.IsAny<QuestionGroupDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateQuestionGroup(questionGroupDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _questionGroupServiceMock.Verify(x => x.CreateQuestionGroup(It.Is<QuestionGroupDto>(qg => qg.Name == "Test Question Group")), Times.Once);
        Console.WriteLine("[DEBUG_LOG] CreateQuestionGroup test passed - Question group created successfully");
    }

    [Fact]
    public async Task GetQuestionGroupsForCurrentUser_ValidRequest_ReturnsQuestionGroups()
    {
        // Arrange
        var expectedQuestionGroups = new List<QuestionGroupDto>
        {
            new() { QuestionGroupId = 1, Name = "Question Group 1", Description = "First group", Questions = [] },
            new() { QuestionGroupId = 2, Name = "Question Group 2", Description = "Second group", Questions = [] }
        };

        _questionGroupServiceMock.Setup(x => x.GetQuestionGroupsForCurrentUser())
            .ReturnsAsync(expectedQuestionGroups);

        // Act
        var result = await _controller.GetQuestionGroupsForCurrentUser();

        // Assert
        var okResult = Assert.IsType<ActionResult<IEnumerable<QuestionGroupDto>>>(result);
        var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var questionGroups = Assert.IsAssignableFrom<IEnumerable<QuestionGroupDto>>(okObjectResult.Value);
        Assert.Equal(2, questionGroups.Count());
        Console.WriteLine("[DEBUG_LOG] GetQuestionGroupsForCurrentUser test passed - Retrieved user's question groups");
    }

    [Fact]
    public async Task GetQuestionGroupsByQuizId_ValidQuizId_ReturnsQuestionGroups()
    {
        // Arrange
        const long quizId = 1;
        var expectedQuestionGroups = new List<QuestionGroupDto>
        {
            new() { QuestionGroupId = 1, Name = "Quiz 1 Group 1", Description = "First group for quiz 1", Questions = [] },
            new() { QuestionGroupId = 2, Name = "Quiz 1 Group 2", Description = "Second group for quiz 1", Questions = [] }
        };

        _questionGroupServiceMock.Setup(x => x.GetQuestionGroupsByQuizId(quizId))
            .ReturnsAsync(expectedQuestionGroups);

        // Act
        var result = await _controller.GetQuestionGroupsByQuizId(quizId);

        // Assert
        var okResult = Assert.IsType<ActionResult<IEnumerable<QuestionGroupDto>>>(result);
        var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var questionGroups = Assert.IsAssignableFrom<IEnumerable<QuestionGroupDto>>(okObjectResult.Value);
        Assert.Equal(2, questionGroups.Count());
        _questionGroupServiceMock.Verify(x => x.GetQuestionGroupsByQuizId(quizId), Times.Once);
        Console.WriteLine("[DEBUG_LOG] GetQuestionGroupsByQuizId test passed - Retrieved question groups for quiz");
    }

    [Fact]
    public async Task GetQuestionGroup_ValidQuestionGroupId_ReturnsQuestionGroup()
    {
        // Arrange
        const long questionGroupId = 1;
        var expectedQuestionGroup = new QuestionGroupDto
        {
            QuestionGroupId = questionGroupId,
            Name = "Test Question Group",
            Description = "Test description",
            Questions = []
        };

        _questionGroupServiceMock.Setup(x => x.GetQuestionGroup(questionGroupId))
            .ReturnsAsync(expectedQuestionGroup);

        // Act
        var result = await _controller.GetQuestionGroup(questionGroupId);

        // Assert
        var okResult = Assert.IsType<ActionResult<QuestionGroupDto>>(result);
        var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var questionGroup = Assert.IsType<QuestionGroupDto>(okObjectResult.Value);
        Assert.Equal(questionGroupId, questionGroup.QuestionGroupId);
        Assert.Equal("Test Question Group", questionGroup.Name);
        _questionGroupServiceMock.Verify(x => x.GetQuestionGroup(questionGroupId), Times.Once);
        Console.WriteLine("[DEBUG_LOG] GetQuestionGroup test passed - Retrieved specific question group");
    }

    [Fact]
    public async Task GetQuestionGroup_InvalidQuestionGroupId_ThrowsBusinessValidationException()
    {
        // Arrange
        const long invalidQuestionGroupId = 999;

        _questionGroupServiceMock.Setup(x => x.GetQuestionGroup(invalidQuestionGroupId))
            .ThrowsAsync(new BusinessValidationException("Question group was not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(() => _controller.GetQuestionGroup(invalidQuestionGroupId));

        Assert.Equal("Question group was not found", exception.Message);
        _questionGroupServiceMock.Verify(x => x.GetQuestionGroup(invalidQuestionGroupId), Times.Once);
        Console.WriteLine("[DEBUG_LOG] GetQuestionGroup invalid ID test passed - Exception thrown correctly");
    }

    [Fact]
    public async Task UpdateQuestionGroup_ValidQuestionGroupDto_ReturnsOk()
    {
        // Arrange
        var questionGroupDto = new QuestionGroupDto
        {
            QuestionGroupId = 1,
            Name = "Updated Question Group",
            Description = "Updated description",
            Questions = []
        };

        _questionGroupServiceMock.Setup(x => x.UpdateQuestionGroup(It.IsAny<QuestionGroupDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateQuestionGroup(questionGroupDto);

        // Assert
        Assert.IsType<OkResult>(result);
        _questionGroupServiceMock.Verify(x => x.UpdateQuestionGroup(It.Is<QuestionGroupDto>(qg =>
            qg.QuestionGroupId == 1 && qg.Name == "Updated Question Group")), Times.Once);
        Console.WriteLine("[DEBUG_LOG] UpdateQuestionGroup test passed - Question group updated successfully");
    }

    [Fact]
    public async Task UpdateQuestionGroup_UnauthorizedUser_ThrowsBusinessValidationException()
    {
        // Arrange
        var questionGroupDto = new QuestionGroupDto
        {
            QuestionGroupId = 1,
            Name = "Unauthorized Update",
            Description = "Should fail",
            Questions = []
        };

        _questionGroupServiceMock.Setup(x => x.UpdateQuestionGroup(It.IsAny<QuestionGroupDto>()))
            .ThrowsAsync(new BusinessValidationException("You can only update question groups that you created"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(() => _controller.UpdateQuestionGroup(questionGroupDto));

        Assert.Equal("You can only update question groups that you created", exception.Message);
        _questionGroupServiceMock.Verify(x => x.UpdateQuestionGroup(It.IsAny<QuestionGroupDto>()), Times.Once);
        Console.WriteLine("[DEBUG_LOG] UpdateQuestionGroup unauthorized test passed - Exception thrown correctly");
    }

    [Fact]
    public async Task DeleteQuestionGroup_ValidQuestionGroupId_ReturnsOk()
    {
        // Arrange
        const long questionGroupId = 1;

        _questionGroupServiceMock.Setup(x => x.DeleteQuestionGroup(questionGroupId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteQuestionGroup(questionGroupId);

        // Assert
        Assert.IsType<OkResult>(result);
        _questionGroupServiceMock.Verify(x => x.DeleteQuestionGroup(questionGroupId), Times.Once);
        Console.WriteLine("[DEBUG_LOG] DeleteQuestionGroup test passed - Question group deleted successfully");
    }

    [Fact]
    public async Task DeleteQuestionGroup_UnauthorizedUser_ThrowsBusinessValidationException()
    {
        // Arrange
        const long questionGroupId = 1;

        _questionGroupServiceMock.Setup(x => x.DeleteQuestionGroup(questionGroupId))
            .ThrowsAsync(new BusinessValidationException("You can only delete question groups that you created"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(() => _controller.DeleteQuestionGroup(questionGroupId));

        Assert.Equal("You can only delete question groups that you created", exception.Message);
        _questionGroupServiceMock.Verify(x => x.DeleteQuestionGroup(questionGroupId), Times.Once);
        Console.WriteLine("[DEBUG_LOG] DeleteQuestionGroup unauthorized test passed - Exception thrown correctly");
    }

    [Fact]
    public async Task GetQuestionGroupsByQuizId_UnauthorizedQuiz_ThrowsBusinessValidationException()
    {
        // Arrange
        const long unauthorizedQuizId = 999;

        _questionGroupServiceMock.Setup(x => x.GetQuestionGroupsByQuizId(unauthorizedQuizId))
            .ThrowsAsync(new BusinessValidationException("You can only access question groups that you created"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(() => _controller.GetQuestionGroupsByQuizId(unauthorizedQuizId));

        Assert.Equal("You can only access question groups that you created", exception.Message);
        _questionGroupServiceMock.Verify(x => x.GetQuestionGroupsByQuizId(unauthorizedQuizId), Times.Once);
        Console.WriteLine("[DEBUG_LOG] GetQuestionGroupsByQuizId unauthorized test passed - Exception thrown correctly for quiz access");
    }
}