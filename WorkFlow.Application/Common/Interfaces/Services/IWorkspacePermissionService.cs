using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Common.Interfaces.Services
{
    public interface IWorkSpacePermissionService
    {
        Task<WorkSpaceRole?> GetRoleAsync(Guid workspaceId, Guid userId);

        Task EnsureMemberAsync(Guid workspaceId, Guid userId);
        Task EnsureAdminAsync(Guid workspaceId, Guid userId);
        Task EnsureOwnerAsync(Guid workspaceId, Guid userId);

        Task EnsureCanModifyMemberRoleAsync(Guid workspaceId, Guid currentUserId, Guid targetUserId);
        Task EnsureCanAssignRoleAsync(Guid workspaceId, Guid currentUserId, WorkSpaceRole newRole);

        Task<bool> IsLastOwnerAsync(Guid workspaceId, Guid userId);
    }
}
