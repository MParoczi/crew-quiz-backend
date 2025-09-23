namespace CrewQuiz.Tests.SignalRTesting;

/// <summary>
///     Simulates multiple concurrent SignalR connections for testing
/// </summary>
public class ConnectionSimulator
{
    private readonly Dictionary<string, (long userId, string sessionId)> _connectionData = [];
    private readonly Dictionary<string, MockHubContext> _connections = [];

    /// <summary>
    ///     Gets the count of active connections
    /// </summary>
    public int ActiveConnectionCount => _connections.Count;

    /// <summary>
    ///     Creates a new simulated connection with the specified connection ID
    /// </summary>
    public MockHubContext CreateConnection(string connectionId, long userId, string sessionId)
    {
        var mockContext = new MockHubContext();
        mockContext.Context.Setup(x => x.ConnectionId).Returns(connectionId);

        _connections[connectionId] = mockContext;
        _connectionData[connectionId] = (userId, sessionId);

        return mockContext;
    }

    /// <summary>
    ///     Gets a connection by its ID
    /// </summary>
    public MockHubContext? GetConnection(string connectionId)
    {
        return _connections.TryGetValue(connectionId, out var connection) ? connection : null;
    }

    /// <summary>
    ///     Gets all active connections
    /// </summary>
    public IReadOnlyDictionary<string, MockHubContext> GetAllConnections()
    {
        return _connections.AsReadOnly();
    }

    /// <summary>
    ///     Simulates a connection disconnect
    /// </summary>
    public void DisconnectConnection(string connectionId)
    {
        _connections.Remove(connectionId);
        _connectionData.Remove(connectionId);
    }

    /// <summary>
    ///     Gets connection data (userId and sessionId) for a specific connection
    /// </summary>
    public (long userId, string sessionId)? GetConnectionData(string connectionId)
    {
        return _connectionData.TryGetValue(connectionId, out var data) ? data : null;
    }

    /// <summary>
    ///     Gets all connections in a specific session
    /// </summary>
    public IEnumerable<string> GetConnectionsInSession(string sessionId)
    {
        return _connectionData
            .Where(kvp => kvp.Value.sessionId == sessionId)
            .Select(kvp => kvp.Key);
    }

    /// <summary>
    ///     Gets all connections for a specific user
    /// </summary>
    public IEnumerable<string> GetConnectionsForUser(long userId)
    {
        return _connectionData
            .Where(kvp => kvp.Value.userId == userId)
            .Select(kvp => kvp.Key);
    }

    /// <summary>
    ///     Simulates multiple users connecting to the same session
    /// </summary>
    public List<MockHubContext> SimulateMultipleUsersInSession(string sessionId, params long[] userIds)
    {
        var connections = new List<MockHubContext>();

        for (var i = 0; i < userIds.Length; i++)
        {
            var connectionId = $"connection-{sessionId}-{userIds[i]}-{i}";
            var connection = CreateConnection(connectionId, userIds[i], sessionId);
            connections.Add(connection);
        }

        return connections;
    }

    /// <summary>
    ///     Verifies that all connections in a session received a specific method call
    /// </summary>
    public void VerifyAllConnectionsInSessionReceived(string sessionId, string methodName, params object[] args)
    {
        var connectionsInSession = GetConnectionsInSession(sessionId);

        foreach (var connectionId in connectionsInSession)
        {
            var connection = GetConnection(connectionId);
            connection?.VerifyGroupCalled(sessionId, methodName, args);
        }
    }

    /// <summary>
    ///     Clears all simulated connections
    /// </summary>
    public void ClearAllConnections()
    {
        _connections.Clear();
        _connectionData.Clear();
    }

    /// <summary>
    ///     Gets the count of connections in a specific session
    /// </summary>
    public int GetConnectionCountInSession(string sessionId)
    {
        return GetConnectionsInSession(sessionId).Count();
    }
}

/// <summary>
///     Utilities for testing connection lifecycle events
/// </summary>
public static class ConnectionLifecycleTestUtilities
{
    /// <summary>
    ///     Simulates a connection being established
    /// </summary>
    public static async Task SimulateConnectionAsync(ConnectionSimulator simulator, string connectionId, long userId, string sessionId)
    {
        var connection = simulator.CreateConnection(connectionId, userId, sessionId);

        // Simulate the connection process
        connection.Context.Setup(x => x.ConnectionId).Returns(connectionId);
        connection.Items["UserId"] = userId;
        connection.Items["SessionId"] = sessionId;

        await Task.CompletedTask;
    }

    /// <summary>
    ///     Simulates a connection being lost unexpectedly
    /// </summary>
    public static async Task SimulateConnectionLossAsync(ConnectionSimulator simulator, string connectionId, Exception? exception = null)
    {
        var connection = simulator.GetConnection(connectionId);
        if (connection != null)
            // Simulate disconnection cleanup
            simulator.DisconnectConnection(connectionId);

        await Task.CompletedTask;
    }

    /// <summary>
    ///     Simulates a graceful disconnection
    /// </summary>
    public static async Task SimulateGracefulDisconnectAsync(ConnectionSimulator simulator, string connectionId)
    {
        await SimulateConnectionLossAsync(simulator, connectionId);
    }
}