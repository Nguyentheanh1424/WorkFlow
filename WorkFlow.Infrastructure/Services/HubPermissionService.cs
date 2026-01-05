using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Infrastructure.Services
{
    public class HubPermissionService : IHubPermissionService
    {
        private readonly IRepository<Board, Guid> _boardRepository;
        private readonly IRepository<BoardMember, Guid> _boardMemberRepository;
        private readonly IRepository<WorkSpace, Guid> _workSpaceRepository;
        private readonly IRepository<WorkspaceMember, Guid> _workSpaceMemberRepository;

        public HubPermissionService(IUnitOfWork uow)
        {
            _boardRepository = uow.GetRepository<Board, Guid>();
            _workSpaceRepository = uow.GetRepository<WorkSpace, Guid>();
            _workSpaceMemberRepository = uow.GetRepository<WorkspaceMember, Guid>();
            _boardMemberRepository = uow.GetRepository<BoardMember, Guid>();
        }

        public async Task<bool> CanAccessBoardAsync(Guid userId, Guid boardId, CancellationToken cancellationToken = default)
        {
            var board = await _boardRepository.FirstOrDefaultAsync(x => x.Id == boardId);
            if (board == null) return false;

            // Public boards
            if (board.Visibility == VisibilityBoard.Public) return true;

            // Owner
            if (board.OwnerId == userId) return true;

            // Members
            var isMember = await _boardMemberRepository.AnyAsync(x => x.BoardId == boardId && x.UserId == userId);

            return isMember;
        }

        public async Task<bool> CanAccessWorkspaceAsync(
            Guid userId,
            Guid workspaceId,
            CancellationToken cancellationToken = default)
        {
            var ws = await _workSpaceRepository.FirstOrDefaultAsync(x => x.Id == workspaceId);

            if (ws == null) return false;

            if (ws.CreatedBy == userId) return true;

            var isMember = await _workSpaceMemberRepository.AnyAsync(
                x => x.WorkSpaceId == workspaceId && x.UserId == userId
            );

            return isMember;
        }
    }
}
