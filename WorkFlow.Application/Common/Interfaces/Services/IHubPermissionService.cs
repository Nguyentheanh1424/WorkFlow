using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkFlow.Application.Common.Interfaces.Services
{
    public interface IHubPermissionService
    {
        Task<bool> CanAccessBoardAsync(Guid userId, Guid boardId, CancellationToken cancellationToken = default);
        Task<bool> CanAccessWorkspaceAsync(Guid userId, Guid workspaceId, CancellationToken cancellationToken = default);
    }
}
