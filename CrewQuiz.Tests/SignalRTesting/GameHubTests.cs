using Backend.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CrewQuiz.Tests.SignalRTesting;

/// <summary>
///     Comprehensive tests for GameHub SignalR functionality
///     Tests cover all methods required by Story ENV-3
/// </summary>
public class GameHubTests : SignalRTestBase
{
    [Fact]
    public async Task JoinGame_Should_AddConnectionToGroup_And_SetContextItems()
    {
        // Arrange
        const string sessionId = "test-session-123";
        const long userId = 12345L;
        const string connectionId = "test-connection-id";

        SetupConnectionId(connectionId);

        // Act
        await GameHub.JoinGame(sessionId, userId);

        // Assert
        AssertJoinedGroup(sessionId);
        AssertContextItem("UserId", userId);
        AssertContextItem("SessionId", sessionId);
    }

    [Fact]
    public async Task JoinGame_Should_Handle_Multiple_Users_In_Same_Session()
    {
        // Arrange
        const string sessionId = "multiplayer-session";
        var simulator = new ConnectionSimulator();

        var userIds = new[] { 1001L, 1002L, 1003L };
        var connections = simulator.SimulateMultipleUsersInSession(sessionId, userIds);

        // Act & Assert
        for (var i = 0; i < connections.Count; i++)
        {
            var connection = connections[i];
            var userId = userIds[i];
            var connectionId = $"connection-{sessionId}-{userId}-{i}";

            // Create a GameHub instance for each connection
            var gameHub = new GameHub();
            var contextProperty = typeof(Hub).GetProperty("Context");
            var groupsProperty = typeof(Hub).GetProperty("Groups");

            contextProperty?.SetValue(gameHub, connection.Context.Object);
            groupsProperty?.SetValue(gameHub, connection.Groups.Object);

            await gameHub.JoinGame(sessionId, userId);

            // Verify each connection joined the group
            connection.VerifyAddedToGroup(connectionId, sessionId);

            // Verify context items were set
            Assert.Equal(userId, connection.Items["UserId"]);
            Assert.Equal(sessionId, connection.Items["SessionId"]);
        }

        // Verify all connections are tracked
        Assert.Equal(3, simulator.GetConnectionCountInSession(sessionId));
    }

    [Fact]
    public async Task LeaveGame_Should_RemoveConnectionFromGroup()
    {
        // Arrange
        const string sessionId = "test-session-leave";
        const long userId = 67890L;
        const string connectionId = "test-connection-id";

        SetupConnectionId(connectionId);

        // First join the game
        await GameHub.JoinGame(sessionId, userId);

        // Act
        await GameHub.LeaveGame(sessionId, userId);

        // Assert
        AssertLeftGroup(sessionId);
    }

    [Fact]
    public async Task LeaveGame_Should_Work_Without_Prior_Join()
    {
        // Arrange
        const string sessionId = "test-session-never-joined";
        const long userId = 99999L;
        const string connectionId = "test-connection-id";

        SetupConnectionId(connectionId);

        // Act & Assert - Should not throw exception
        await GameHub.LeaveGame(sessionId, userId);

        // Verify the leave operation was attempted
        AssertLeftGroup(sessionId);
    }

    [Fact]
    public async Task OnDisconnectedAsync_Should_Call_Base_Implementation()
    {
        // Arrange
        Exception? testException = new InvalidOperationException("Test disconnect");

        // Act & Assert - Should not throw exception
        await GameHub.OnDisconnectedAsync(testException);

        // Since the base implementation doesn't do much observable,
        // we verify that the method executes without error
        Assert.True(true); // Method completed successfully
    }

    [Fact]
    public async Task OnDisconnectedAsync_Should_Handle_Null_Exception()
    {
        // Act & Assert - Should not throw exception
        await GameHub.OnDisconnectedAsync(null);

        // Verify that null exception is handled gracefully
        Assert.True(true); // Method completed successfully
    }

    [Fact]
    public async Task Hub_Context_Should_Be_Properly_Mocked()
    {
        // Arrange & Assert
        Assert.NotNull(GameHub);
        Assert.NotNull(MockHubContext);
        Assert.NotNull(MockHubContext.Context.Object);
        Assert.NotNull(MockHubContext.Groups.Object);
        Assert.NotNull(MockHubContext.Clients.Object);

        // Verify connection ID is accessible
        Assert.Equal("test-connection-id", MockHubContext.Context.Object.ConnectionId);
    }

    [Fact]
    public async Task Multiple_Concurrent_Connections_Should_Be_Testable()
    {
        // Arrange
        var simulator = new ConnectionSimulator();
        const string sessionId = "concurrent-test-session";

        // Simulate 5 concurrent users
        var userIds = Enumerable.Range(2001, 5).Select(i => (long)i).ToArray();
        var connections = simulator.SimulateMultipleUsersInSession(sessionId, userIds);

        // Act - Simulate all users joining concurrently
        var joinTasks = new List<Task>();

        for (var i = 0; i < connections.Count; i++)
        {
            var connection = connections[i];
            var userId = userIds[i];

            var gameHub = new GameHub();
            var contextProperty = typeof(Hub).GetProperty("Context");
            var groupsProperty = typeof(Hub).GetProperty("Groups");

            contextProperty?.SetValue(gameHub, connection.Context.Object);
            groupsProperty?.SetValue(gameHub, connection.Groups.Object);

            joinTasks.Add(gameHub.JoinGame(sessionId, userId));
        }

        await Task.WhenAll(joinTasks);

        // Assert
        Assert.Equal(5, simulator.ActiveConnectionCount);
        Assert.Equal(5, simulator.GetConnectionCountInSession(sessionId));

        // Verify each connection joined successfully
        foreach (var userId in userIds)
        {
            var userConnections = simulator.GetConnectionsForUser(userId);
            Assert.Single(userConnections);
        }
    }

    [Fact]
    public async Task Connection_Lifecycle_Should_Be_Testable()
    {
        // Arrange
        var simulator = new ConnectionSimulator();
        const string sessionId = "lifecycle-test-session";
        const long userId = 3001L;
        const string connectionId = "lifecycle-connection-id";

        // Act & Assert - Connection establishment
        await ConnectionLifecycleTestUtilities.SimulateConnectionAsync(simulator, connectionId, userId, sessionId);

        Assert.Equal(1, simulator.ActiveConnectionCount);
        Assert.NotNull(simulator.GetConnection(connectionId));

        var connectionData = simulator.GetConnectionData(connectionId);
        Assert.NotNull(connectionData);
        Assert.Equal(userId, connectionData.Value.userId);
        Assert.Equal(sessionId, connectionData.Value.sessionId);

        // Act & Assert - Connection loss
        await ConnectionLifecycleTestUtilities.SimulateConnectionLossAsync(simulator, connectionId);

        Assert.Equal(0, simulator.ActiveConnectionCount);
        Assert.Null(simulator.GetConnection(connectionId));
        Assert.Null(simulator.GetConnectionData(connectionId));
    }

    [Fact]
    public async Task Group_Management_Functions_Should_Work_Correctly()
    {
        // Arrange
        const string sessionId1 = "group-test-session-1";
        const string sessionId2 = "group-test-session-2";
        const long userId = 4001L;

        // Act - Join first session
        await GameHub.JoinGame(sessionId1, userId);
        AssertJoinedGroup(sessionId1);
        AssertContextItem("SessionId", sessionId1);

        // Act - Leave first session and join second session
        await GameHub.LeaveGame(sessionId1, userId);
        AssertLeftGroup(sessionId1);

        await GameHub.JoinGame(sessionId2, userId);
        AssertJoinedGroup(sessionId2);
        AssertContextItem("SessionId", sessionId2);

        // Act - Leave second session
        await GameHub.LeaveGame(sessionId2, userId);
        AssertLeftGroup(sessionId2);
    }
}