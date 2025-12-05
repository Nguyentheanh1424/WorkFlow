using Microsoft.AspNetCore.SignalR;

namespace WorkFlow.API.Hubs
{
    public class UserHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("[HUB] CONNECTED");
            Console.WriteLine("ConnectionId = " + Context.ConnectionId);

            var userId = Context.User?.FindFirst("userId")?.Value;

            if (!string.IsNullOrWhiteSpace(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
                Console.WriteLine($"Joined group user:{userId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst("userId")?.Value;

            if (!string.IsNullOrWhiteSpace(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user:{userId}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }

}
