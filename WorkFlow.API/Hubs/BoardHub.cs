using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WorkFlow.Application.Common.Interfaces.Services;

namespace WorkFlow.API.Hubs
{
    public class BoardHub : Hub
    {
        private readonly IHubPermissionService _hubPermissionService;

        public BoardHub(IHubPermissionService permission)
        {
            _hubPermissionService = permission;
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"[BoardHub] Connection {Context.ConnectionId} connected");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"[BoardHub] Connection {Context.ConnectionId} disconnected");
            return base.OnDisconnectedAsync(exception);
        }

        public async Task JoinBoard(string boardId)
        {
            if (!Guid.TryParse(boardId, out var boardGuid))
                throw new HubException("Invalid board id");

            var userId = Context.User?.FindFirst("userId")?.Value;
            if (!Guid.TryParse(userId, out var uid))
                throw new HubException("Unauthenticated");

            var allowed = await _hubPermissionService.CanAccessBoardAsync(uid, boardGuid);
            if (!allowed)
                throw new HubException("Forbidden");

            Console.WriteLine($"[BoardHub] {Context.ConnectionId} joined board:{boardId}");
            await Groups.AddToGroupAsync(Context.ConnectionId, $"board:{boardId}");
        }

        public async Task LeaveBoard(string boardId)
        {
            if (string.IsNullOrWhiteSpace(boardId))
                return;

            Console.WriteLine($"[BoardHub] {Context.ConnectionId} left board:{boardId}");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"board:{boardId}");
        }
    }
}
