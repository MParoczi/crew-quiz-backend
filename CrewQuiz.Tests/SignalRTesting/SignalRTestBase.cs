using Backend.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CrewQuiz.Tests.SignalRTesting;

/// <summary>
///     Base class for SignalR hub testing that provides common setup and utilities
/// </summary>
public class SignalRTestBase : TestBase
{
    public SignalRTestBase()
    {
        SetupSignalRTesting();
    }

    protected MockHubContext MockHubContext { get; private set; }
    protected GameHub GameHub { get; private set; }

    private void SetupSignalRTesting()
    {
        // Create mock hub context
        MockHubContext = new MockHubContext();

        // Create GameHub instance and inject mock context
        GameHub = new GameHub();

        // Use reflection to set the Context property since it's protected
        var contextProperty = typeof(Hub).GetProperty("Context");
        contextProperty?.SetValue(GameHub, MockHubContext.Context.Object);

        // Use reflection to set the Groups property since it's protected
        var groupsProperty = typeof(Hub).GetProperty("Groups");
        groupsProperty?.SetValue(GameHub, MockHubContext.Groups.Object);

        // Use reflection to set the Clients property since it's protected
        var clientsProperty = typeof(Hub).GetProperty("Clients");
        clientsProperty?.SetValue(GameHub, MockHubContext.Clients.Object);
    }

    /// <summary>
    ///     Creates a new mock hub context with fresh state
    /// </summary>
    protected MockHubContext CreateFreshMockContext()
    {
        return new MockHubContext();
    }

    /// <summary>
    ///     Sets up a specific connection ID for testing
    /// </summary>
    protected void SetupConnectionId(string connectionId)
    {
        MockHubContext.Context.Setup(x => x.ConnectionId).Returns(connectionId);
    }

    /// <summary>
    ///     Sets up context items for testing
    /// </summary>
    protected void SetupContextItems(string key, object value)
    {
        MockHubContext.Items[key] = value;
    }

    /// <summary>
    ///     Verifies that the hub joined a specific group
    /// </summary>
    protected void AssertJoinedGroup(string sessionId)
    {
        var currentConnectionId = MockHubContext.Context.Object.ConnectionId;
        MockHubContext.VerifyAddedToGroup(currentConnectionId, sessionId);
    }

    /// <summary>
    ///     Verifies that the hub left a specific group
    /// </summary>
    protected void AssertLeftGroup(string sessionId)
    {
        var currentConnectionId = MockHubContext.Context.Object.ConnectionId;
        MockHubContext.VerifyRemovedFromGroup(currentConnectionId, sessionId);
    }

    /// <summary>
    ///     Verifies that context items were set correctly
    /// </summary>
    protected void AssertContextItem(string key, object expectedValue)
    {
        Assert.True(MockHubContext.Items.ContainsKey(key));
        Assert.Equal(expectedValue, MockHubContext.Items[key]);
    }

    /// <summary>
    ///     Verifies that a method was broadcasted to all clients
    /// </summary>
    protected void AssertBroadcastToAll(string methodName, params object[] args)
    {
        MockHubContext.VerifyAllClientsCalled(methodName, args);
    }

    /// <summary>
    ///     Verifies that a method was broadcasted to a specific group
    /// </summary>
    protected void AssertBroadcastToGroup(string groupName, string methodName, params object[] args)
    {
        MockHubContext.VerifyGroupCalled(groupName, methodName, args);
    }
}