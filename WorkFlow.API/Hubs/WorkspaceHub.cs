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
    public class WorkspaceHub : Hub
    {
        private readonly IHubPermissionService _hubPermissionService;

        public WorkspaceHub(IHubPermissionService permission)
        {
            _hubPermissionService = permission;
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"[WSHub] Connection {Context.ConnectionId} connected");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"[WSHub] Connection {Context.ConnectionId} disconnected");
            return base.OnDisconnectedAsync(exception);
        }

        public async Task JoinWorkspace(string workspaceId)
        {
            if (!Guid.TryParse(workspaceId, out var wsGuid))
                throw new HubException("Invalid workspace id");

            var userId = Context.User?.FindFirst("userId")?.Value;
            Console.WriteLine(userId);
            if (!Guid.TryParse(userId, out var uid))
                throw new HubException("Unauthenticated");

            var allowed = await _hubPermissionService.CanAccessWorkspaceAsync(uid, wsGuid);
            if (!allowed)
                throw new HubException("Forbidden");

            Console.WriteLine($"[WSHub] {Context.ConnectionId} joined ws:{workspaceId}");
            await Groups.AddToGroupAsync(Context.ConnectionId, $"ws:{workspaceId}");
        }

        public async Task LeaveWorkspace(string workspaceId)
        {
            if (string.IsNullOrWhiteSpace(workspaceId))
                return;

            Console.WriteLine($"[WSHub] {Context.ConnectionId} left ws:{workspaceId}");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ws:{workspaceId}");
        }
    }
}
