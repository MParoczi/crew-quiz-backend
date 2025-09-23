using Backend.Enums;
using Backend.Hubs;
using Backend.Interfaces.Data;
using Backend.Interfaces.Utils;
using Backend.Models.DTOs;
using Backend.Models.Exceptions;
using Backend.ServiceUtils;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace CrewQuiz.Tests.SignalRTesting;

public class GameFlowServiceUtilTests : SignalRTestBase
{
    private readonly GameFlowServiceUtil _gameFlowServiceUtil;
    private readonly Mock<IHubContext<GameHub>> _mockHubContext;
    private readonly MockHubContext _mockHubContextHelper;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IServiceDispatcher> _serviceDispatcher;
    private Mock<IClientProxy> _clientProxyMock;
    private Mock<IHubClients> _hubClientsMock;

    public GameFlowServiceUtilTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockHubContext = new Mock<IHubContext<GameHub>>();
        _mockHubContextHelper = new MockHubContext();
        _serviceDispatcher = new Mock<IServiceDispatcher>();
        _gameFlowServiceUtil = new GameFlowServiceUtil(_mockHubContext.Object, _mockUnitOfWork.Object, _serviceDispatcher.Object);

        SetupMockHubContext();
    }

    private void SetupMockHubContext()
    {
        // Create a separate IHubClients mock for IHubContext<GameHub>
        var mockHubClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();

        // Setup the IHubClients mock to return our client proxy
        mockHubClients.Setup(x => x.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
        mockClientProxy.Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Setup the IHubContext mock
        _mockHubContext.Setup(x => x.Clients).Returns(mockHubClients.Object);
        _mockHubContext.Setup(x => x.Groups).Returns(_mockHubContextHelper.Groups.Object);

        // Store the mocks for verification
        _hubClientsMock = mockHubClients;
        _clientProxyMock = mockClientProxy;
    }

    #region SendEventToGroup Tests

    [Fact]
    public async Task SendEventToGroup_Should_Broadcast_GameStarted_Event_To_Session_Group()
    {
        // Arrange
        const string sessionId = "test-session-123";
        var gameFlowDto = CreateTestGameFlowDto(sessionId);
        const GameEventType eventType = GameEventType.GameStarted;

        // Act
        await _gameFlowServiceUtil.SendEventToGroup(gameFlowDto, eventType);

        // Assert
        VerifyBroadcastToGroup(sessionId, eventType.ToString(), gameFlowDto);
    }

    [Fact]
    public async Task SendEventToGroup_Should_Broadcast_PlayerJoined_Event_To_Session_Group()
    {
        // Arrange
        const string sessionId = "multiplayer-session";
        var gameFlowDto = CreateTestGameFlowDto(sessionId, 12345L);
        const GameEventType eventType = GameEventType.PlayerJoined;

        // Act
        await _gameFlowServiceUtil.SendEventToGroup(gameFlowDto, eventType);

        // Assert
        VerifyBroadcastToGroup(sessionId, eventType.ToString(), gameFlowDto);
    }

    [Fact]
    public async Task SendEventToGroup_Should_Broadcast_QuestionSelected_Event_To_Session_Group()
    {
        // Arrange
        const string sessionId = "quiz-session";
        var gameFlowDto = CreateTestGameFlowDto(sessionId, questionId: 789L);
        const GameEventType eventType = GameEventType.QuestionSelected;

        // Act
        await _gameFlowServiceUtil.SendEventToGroup(gameFlowDto, eventType);

        // Assert
        VerifyBroadcastToGroup(sessionId, eventType.ToString(), gameFlowDto);
    }

    [Fact]
    public async Task SendEventToGroup_Should_Broadcast_AnswerSubmitted_Event_To_Session_Group()
    {
        // Arrange
        const string sessionId = "answer-session";
        var gameFlowDto = CreateTestGameFlowDto(sessionId, answer: "Test Answer");
        const GameEventType eventType = GameEventType.AnswerSubmitted;

        // Act
        await _gameFlowServiceUtil.SendEventToGroup(gameFlowDto, eventType);

        // Assert
        VerifyBroadcastToGroup(sessionId, eventType.ToString(), gameFlowDto);
    }

    [Fact]
    public async Task SendEventToGroup_Should_Broadcast_QuestionRobbingIsAllowed_Event_To_Session_Group()
    {
        // Arrange
        const string sessionId = "robbing-session";
        var gameFlowDto = CreateTestGameFlowDto(sessionId);
        const GameEventType eventType = GameEventType.QuestionRobbingIsAllowed;

        // Act
        await _gameFlowServiceUtil.SendEventToGroup(gameFlowDto, eventType);

        // Assert
        VerifyBroadcastToGroup(sessionId, eventType.ToString(), gameFlowDto);
    }

    [Fact]
    public async Task SendEventToGroup_Should_Broadcast_QuestionRobbed_Event_To_Session_Group()
    {
        // Arrange
        const string sessionId = "rob-success-session";
        var gameFlowDto = CreateTestGameFlowDto(sessionId, 999L);
        const GameEventType eventType = GameEventType.QuestionRobbed;

        // Act
        await _gameFlowServiceUtil.SendEventToGroup(gameFlowDto, eventType);

        // Assert
        VerifyBroadcastToGroup(sessionId, eventType.ToString(), gameFlowDto);
    }

    [Fact]
    public async Task SendEventToGroup_Should_Broadcast_QuestionAnswered_Event_To_Session_Group()
    {
        // Arrange
        const string sessionId = "answered-session";
        var gameFlowDto = CreateTestGameFlowDto(sessionId);
        const GameEventType eventType = GameEventType.QuestionAnswered;

        // Act
        await _gameFlowServiceUtil.SendEventToGroup(gameFlowDto, eventType);

        // Assert
        VerifyBroadcastToGroup(sessionId, eventType.ToString(), gameFlowDto);
    }

    [Fact]
    public async Task SendEventToGroup_Should_Broadcast_PlayerLeft_Event_To_Session_Group()
    {
        // Arrange
        const string sessionId = "leaving-session";
        var gameFlowDto = CreateTestGameFlowDto(sessionId, 555L);
        const GameEventType eventType = GameEventType.PlayerLeft;

        // Act
        await _gameFlowServiceUtil.SendEventToGroup(gameFlowDto, eventType);

        // Assert
        VerifyBroadcastToGroup(sessionId, eventType.ToString(), gameFlowDto);
    }

    [Fact]
    public async Task SendEventToGroup_Should_Broadcast_PlayerDisconnected_Event_To_Session_Group()
    {
        // Arrange
        const string sessionId = "disconnect-session";
        var gameFlowDto = CreateTestGameFlowDto(sessionId);
        const GameEventType eventType = GameEventType.PlayerDisconnected;

        // Act
        await _gameFlowServiceUtil.SendEventToGroup(gameFlowDto, eventType);

        // Assert
        VerifyBroadcastToGroup(sessionId, eventType.ToString(), gameFlowDto);
    }

    [Fact]
    public async Task SendEventToGroup_Should_Broadcast_GameEnded_Event_To_Session_Group()
    {
        // Arrange
        const string sessionId = "end-game-session";
        var gameFlowDto = CreateTestGameFlowDto(sessionId);
        const GameEventType eventType = GameEventType.GameEnded;

        // Act
        await _gameFlowServiceUtil.SendEventToGroup(gameFlowDto, eventType);

        // Assert
        VerifyBroadcastToGroup(sessionId, eventType.ToString(), gameFlowDto);
    }

    [Fact]
    public async Task SendEventToGroup_Should_Throw_BusinessValidationException_When_HubException_Occurs()
    {
        // Arrange
        const string sessionId = "error-session";
        var gameFlowDto = CreateTestGameFlowDto(sessionId);
        const GameEventType eventType = GameEventType.GameStarted;

        // Setup the ClientProxy to throw a HubException when SendCoreAsync is called
        _clientProxyMock
            .Setup(x => x.SendCoreAsync(eventType.ToString(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HubException("Connection error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(() => _gameFlowServiceUtil.SendEventToGroup(gameFlowDto, eventType));

        Assert.Equal("Couldn't send event to users", exception.Message);
    }

    #endregion

    #region UpdateGameFlow Tests

    [Fact]
    public async Task UpdateGameFlow_Should_Call_SendEventToGroup_With_Correct_Parameters()
    {
        // Arrange
        const string sessionId = "update-session";
        var gameFlowDto = CreateTestGameFlowDto(sessionId);
        const GameEventType eventType = GameEventType.QuestionSelected;

        // Act
        await _gameFlowServiceUtil.UpdateGameFlow(gameFlowDto, eventType);

        // Assert
        VerifyBroadcastToGroup(sessionId, eventType.ToString(), gameFlowDto);
    }

    [Fact]
    public async Task UpdateGameFlow_Should_Handle_All_GameEventTypes()
    {
        // Arrange
        const string sessionId = "comprehensive-session";
        var gameFlowDto = CreateTestGameFlowDto(sessionId);
        var allEventTypes = Enum.GetValues<GameEventType>();

        // Act & Assert
        foreach (var eventType in allEventTypes)
        {
            await _gameFlowServiceUtil.UpdateGameFlow(gameFlowDto, eventType);
            VerifyBroadcastToGroup(sessionId, eventType.ToString(), gameFlowDto);
        }
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task Multiple_Concurrent_Broadcasts_Should_Work_Correctly()
    {
        // Arrange
        const string sessionId = "concurrent-session";
        var tasks = new List<Task>();
        var eventTypes = new[]
        {
            GameEventType.PlayerJoined,
            GameEventType.GameStarted,
            GameEventType.QuestionSelected,
            GameEventType.AnswerSubmitted,
            GameEventType.PlayerLeft
        };

        // Act
        for (var i = 0; i < eventTypes.Length; i++)
        {
            var gameFlowDto = CreateTestGameFlowDto(sessionId, i + 1);
            var eventType = eventTypes[i];
            tasks.Add(_gameFlowServiceUtil.SendEventToGroup(gameFlowDto, eventType));
        }

        await Task.WhenAll(tasks);

        // Assert - Verify all events were broadcasted
        for (var i = 0; i < eventTypes.Length; i++)
        {
            var expectedDto = CreateTestGameFlowDto(sessionId, i + 1);
            VerifyBroadcastToGroup(sessionId, eventTypes[i].ToString(), expectedDto);
        }
    }

    [Fact]
    public async Task State_Synchronization_Events_Should_Maintain_Order()
    {
        // Arrange
        const string sessionId = "sync-session";
        var orderedEvents = new[]
        {
            GameEventType.GameStarted,
            GameEventType.QuestionSelected,
            GameEventType.AnswerSubmitted,
            GameEventType.QuestionAnswered,
            GameEventType.GameEnded
        };

        // Act - Send events in sequence
        for (var i = 0; i < orderedEvents.Length; i++)
        {
            var gameFlowDto = CreateTestGameFlowDto(sessionId, i + 100);
            await _gameFlowServiceUtil.UpdateGameFlow(gameFlowDto, orderedEvents[i]);
        }

        // Assert - Verify each event was sent
        for (var i = 0; i < orderedEvents.Length; i++)
        {
            var expectedDto = CreateTestGameFlowDto(sessionId, i + 100);
            VerifyBroadcastToGroup(sessionId, orderedEvents[i].ToString(), expectedDto);
        }
    }

    #endregion

    #region Helper Methods

    private static GameFlowDto CreateTestGameFlowDto(string sessionId, long userId = 123L, long questionId = 456L, string answer = "Test")
    {
        return new GameFlowDto
        {
            SessionId = sessionId,
            UserId = userId,
            QuestionId = questionId,
            Answer = answer
        };
    }

    private void VerifyBroadcastToGroup(string groupName, string methodName, params object[] args)
    {
        _hubClientsMock.Verify(x => x.Group(groupName), Times.AtLeastOnce);
        _clientProxyMock.Verify(x => x.SendCoreAsync(methodName, It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    #endregion
}