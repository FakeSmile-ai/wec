using Microsoft.AspNetCore.SignalR;

namespace MatchesService.Hubs;

public class MatchHub : Hub
{
    public static string GroupName(int matchId) => $"match-{matchId}";

    public override async Task OnConnectedAsync()
    {
        if (Context.GetHttpContext() is { } http && int.TryParse(http.Request.Query["matchId"], out var matchId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(matchId));
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.GetHttpContext() is { } http && int.TryParse(http.Request.Query["matchId"], out var matchId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(matchId));
        }

        await base.OnDisconnectedAsync(exception);
    }

    public Task JoinMatch(int matchId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, GroupName(matchId));
    }

    public Task LeaveMatch(int matchId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(matchId));
    }
}
