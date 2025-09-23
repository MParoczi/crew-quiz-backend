using Microsoft.AspNetCore.SignalR;
using Moq;

namespace CrewQuiz.Tests.SignalRTesting;

/// <summary>
///     Provides mock implementations for SignalR Hub testing
/// </summary>
public class MockHubContext
{
    public MockHubContext()
    {
        Context = new Mock<HubCallerContext>();
        Groups = new Mock<IGroupManager>();
        Clients = new Mock<IHubCallerClients>();
        SingleClientProxy = new Mock<ISingleClientProxy>();
        GroupClientProxy = new Mock<IClientProxy>();
        Items = new Dictionary<object, object?>();

        SetupMockBehavior();
    }

    public Mock<HubCallerContext> Context { get; }
    public Mock<IGroupManager> Groups { get; }
    public Mock<IHubCallerClients> Clients { get; }
    public Mock<ISingleClientProxy> SingleClientProxy { get; }
    public Mock<IClientProxy> GroupClientProxy { get; }
    public Dictionary<object, object?> Items { get; }

    private void SetupMockBehavior()
    {
        // Setup connection ID
        Context.Setup(x => x.ConnectionId).Returns("test-connection-id");

        // Setup Items dictionary
        Context.Setup(x => x.Items).Returns(Items);

        // Setup Groups mock to return completed tasks
        Groups.Setup(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        Groups.Setup(x => x.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Setup client proxies to return completed tasks
        GroupClientProxy.Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        SingleClientProxy.Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Setup clients to return appropriate proxy objects
        Clients.Setup(x => x.All).Returns(GroupClientProxy.Object);
        Clients.Setup(x => x.Group(It.IsAny<string>())).Returns(GroupClientProxy.Object);
        Clients.Setup(x => x.Client(It.IsAny<string>())).Returns(SingleClientProxy.Object);
    }

    /// <summary>
    ///     Verifies that a connection was added to a specific group
    /// </summary>
    public void VerifyAddedToGroup(string connectionId, string groupName)
    {
        Groups.Verify(x => x.AddToGroupAsync(connectionId, groupName, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    ///     Verifies that a connection was removed from a specific group
    /// </summary>
    public void VerifyRemovedFromGroup(string connectionId, string groupName)
    {
        Groups.Verify(x => x.RemoveFromGroupAsync(connectionId, groupName, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    ///     Verifies that a method was called on all clients
    /// </summary>
    public void VerifyAllClientsCalled(string methodName, params object[] args)
    {
        GroupClientProxy.Verify(x => x.SendCoreAsync(methodName, args, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    ///     Verifies that a method was called on a specific group
    /// </summary>
    public void VerifyGroupCalled(string groupName, string methodName, params object[] args)
    {
        Clients.Verify(x => x.Group(groupName), Times.Once);
        GroupClientProxy.Verify(x => x.SendCoreAsync(methodName, args, It.IsAny<CancellationToken>()), Times.Once);
    }
}