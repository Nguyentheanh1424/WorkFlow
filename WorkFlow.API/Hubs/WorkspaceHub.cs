using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WorkFlow.API.Hubs
{
    public class WorkspaceHub : Hub
    {
        public Task JoinWorkspace(string workspaceId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, $"ws:{workspaceId}");
        }

        public Task LeaveWorkspace(string workspaceId)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ws:{workspaceId}");
        }
    }
}
