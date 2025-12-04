using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.API.Hubs;
using WorkFlow.Application.Common.Interfaces.Services;

namespace WorkFlow.Infrastructure.Services
{
    public class RealtimeService : IRealtimeService
    {
        private readonly IHubContext<BoardHub> _boardHub;
        private readonly IHubContext<WorkspaceHub> _workspaceHub;
        private readonly IHubContext<UserHub> _userHub;

        public RealtimeService(
            IHubContext<BoardHub> boardHub,
            IHubContext<WorkspaceHub> workspaceHub,
            IHubContext<UserHub> userHub)
        {
            _boardHub = boardHub;
            _workspaceHub = workspaceHub;
            _userHub = userHub;
        }

        public Task SendToBoardAsync(Guid boardId, string method, object payload)
        {
            var groupName = $"board:{boardId}";
            return _boardHub.Clients.Group(groupName).SendAsync(method, payload);
        }

        public Task SendToWorkspaceAsync(Guid workspaceId, string method, object payload)
        {
            var groupName = $"ws:{workspaceId}";
            return _workspaceHub.Clients.Group(groupName).SendAsync(method, payload);
        }

        public Task SendToUserAsync(Guid userId, string method, object payload)
        {
            var groupName = $"user:{userId}";
            return _userHub.Clients.Group(groupName).SendAsync(method, payload);
        }
    }
}
