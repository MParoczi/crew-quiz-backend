using Microsoft.AspNetCore.SignalR;

namespace Backend.Hubs;

public class GameHub : Hub
{
    public async Task JoinGame(string sessionId, long userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);

        Context.Items["UserId"] = userId;
        Context.Items["SessionId"] = sessionId;
    }

    public async Task LeaveGame(string sessionId, long userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.Items.TryGetValue("UserId", out var userIdObj) ? userIdObj : null;
        var sessionId = Context.Items.TryGetValue("SessionId", out var sessionIdObj) ? sessionIdObj : null;

        await base.OnDisconnectedAsync(exception);
    }
}