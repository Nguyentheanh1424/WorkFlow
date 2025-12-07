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
    }
}
