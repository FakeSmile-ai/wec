using Microsoft.AspNetCore.SignalR;

namespace MatchesService.Hubs
{
    public class ScoreHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var http = Context.GetHttpContext();
            var matchId = http?.Request.Query["matchId"].ToString();

            if (!string.IsNullOrWhiteSpace(matchId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"match-{matchId}");
                await Clients.Group($"match-{matchId}")
                    .SendAsync("userJoined", new { connectionId = Context.ConnectionId });
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var http = Context.GetHttpContext();
            var matchId = http?.Request.Query["matchId"].ToString();

            if (!string.IsNullOrWhiteSpace(matchId))
            {
                await Clients.Group($"match-{matchId}")
                    .SendAsync("userLeft", new { connectionId = Context.ConnectionId });
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
