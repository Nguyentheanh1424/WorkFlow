using System;
using System.Threading.Tasks;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Common.Interfaces.Services
{
    public interface IWorkspacePermissionService
    {
        Task<WorkSpaceRole?> GetRoleAsync(Guid workspaceId, Guid userId);

        Task EnsureMemberAsync(Guid workspaceId, Guid userId);
        Task EnsureAdminAsync(Guid workspaceId, Guid userId);
        Task EnsureOwnerAsync(Guid workspaceId, Guid userId);
    }
}
