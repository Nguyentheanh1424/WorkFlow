using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkFlow.Application.Common.Interfaces.Services
{
    public interface IRealtimeService
    {
        /// <summary>
        /// Sends a message with the specified method and payload to the board identified by the given ID
        /// asynchronously.
        /// </summary>
        /// <param name="boardId">The unique identifier of the target board to which the message will be sent.</param>
        /// <param name="method">The name of the method or action to invoke on the board. Cannot be null or empty.</param>
        /// <param name="payload">The data to include with the message. May be null if the method does not require a payload.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        Task SendToBoardAsync(Guid boardId, string method, object payload);

        /// <summary>
        /// Sends a message with the specified method and payload to the given workspace asynchronously.
        /// </summary>
        /// <param name="workspaceId">The unique identifier of the workspace to which the message will be sent.</param>
        /// <param name="method">The name of the method or action to invoke in the target workspace. Cannot be null or empty.</param>
        /// <param name="payload">The payload object to include with the message. May be null if the method does not require additional data.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        Task SendToWorkspaceAsync(Guid workspaceId, string method, object payload);

        /// <summary>
        /// Asynchronously sends a message with the specified method and payload to a single user identified by user ID.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to whom the message will be sent.</param>
        /// <param name="method">The name of the method or event to invoke on the client. Cannot be null or empty.</param>
        /// <param name="payload">The data to send to the user. May be null if the method does not require a payload.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        Task SendToUserAsync(Guid userId, string method, object payload);
    }
}





















































