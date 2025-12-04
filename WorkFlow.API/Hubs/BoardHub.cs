using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WorkFlow.API.Hubs
{
    public class BoardHub : Hub
    {
        /// <summary>
        /// Adds the current connection to the specified board group, enabling the user to receive messages sent to that
        /// board.
        /// </summary>
        /// <param name="boardId">The unique identifier of the board to join. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous join operation.</returns>
        public Task JoinBoard(string boardId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, $"board:{boardId}");
        }

        public Task LeaveBoard(string boardId)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, $"board:{boardId}");
        }
    }
}
