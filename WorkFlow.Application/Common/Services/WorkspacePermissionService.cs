using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;
using Task = System.Threading.Tasks.Task;

namespace WorkFlow.Application.Common.Services
{
    public class WorkspacePermissionService : IWorkspacePermissionService
    {
        private readonly IRepository<WorkspaceMember, Guid> _workspaceMemberRepository;

        public WorkspacePermissionService(IUnitOfWork unitOfWork)
        {
            _workspaceMemberRepository = unitOfWork.GetRepository<WorkspaceMember, Guid>();
        }

        public async Task<WorkSpaceRole?> GetRoleAsync(Guid workspaceId, Guid userId)
        {
            var member = await _workspaceMemberRepository.FirstOrDefaultAsync(
                x => x.WorkSpaceId == workspaceId && x.UserId == userId
            );

            return member?.Role;
        }

        public async Task EnsureMemberAsync(Guid workspaceId, Guid userId)
        {
            var role = await GetRoleAsync(workspaceId, userId);

            if (role == null)
                throw new ForbiddenAccessException("Bạn không thuộc WorkSpace này.");
        }

        public async Task EnsureAdminAsync(Guid workspaceId, Guid userId)
        {
            var role = await GetRoleAsync(workspaceId, userId);

            if (role is null or WorkSpaceRole.Member)
                throw new ForbiddenAccessException("Bạn không có quyền quản lý WorkSpace.");
        }

        public async Task EnsureOwnerAsync(Guid workspaceId, Guid userId)
        {
            var role = await GetRoleAsync(workspaceId, userId);

            if (role != WorkSpaceRole.Owner)
                throw new ForbiddenAccessException("Chỉ chủ WorkSpace mới được thực hiện hành động này.");
        }

        private int Rank(WorkSpaceRole role) => role switch
        {
            WorkSpaceRole.Owner => 3,
            WorkSpaceRole.Admin => 2,
            WorkSpaceRole.Member => 1,
            _ => 0
        };

        public async Task EnsureCanModifyMemberRoleAsync(Guid workspaceId, Guid currentUserId, Guid targetUserId)
        {
            var currentRole = await GetRoleAsync(workspaceId, currentUserId)
                ?? throw new ForbiddenAccessException("Bạn không thuộc Workspace.");

            var targetRole = await GetRoleAsync(workspaceId, targetUserId);

            // Nếu target chưa join workspace → không cần check hierarchy
            if (targetRole is null)
                return;

            if (Rank(targetRole.Value) > Rank(currentRole))
                throw new ForbiddenAccessException("Không thể chỉnh sửa thành viên có quyền cao hơn bạn.");
        }

        public async Task EnsureCanAssignRoleAsync(Guid workspaceId, Guid currentUserId, WorkSpaceRole newRole)
        {
            var currentRole = await GetRoleAsync(workspaceId, currentUserId)
                ?? throw new ForbiddenAccessException("Bạn không thuộc Workspace.");

            if (Rank(newRole) > Rank(currentRole))
                throw new ForbiddenAccessException("Bạn không thể gán quyền cao hơn quyền của bạn.");
        }

        public async Task<bool> IsLastOwnerAsync(Guid workspaceId, Guid userId)
        {
            var role = await GetRoleAsync(workspaceId, userId);

            if (role != WorkSpaceRole.Owner)
                return false;

            var ownerCount = await _workspaceMemberRepository.CountAsync(
                x => x.WorkSpaceId == workspaceId && x.Role == WorkSpaceRole.Owner
            );

            return ownerCount <= 1;
        }
    }
}
