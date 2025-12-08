using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;
using Task = System.Threading.Tasks.Task;

namespace WorkFlow.Application.Common.Services
{
    public class CardPermissionService : ICardPermissionService
    {
        private readonly IRepository<Card, Guid> _cardRepository;
        private readonly IRepository<CardAssignee, Guid> _assigneeRepository;
        private readonly IRepository<Board, Guid> _boardRepository;
        private readonly IRepository<List, Guid> _listRepository;
        private readonly IBoardPermissionService _boardPermission;
        private readonly IWorkspacePermissionService _workspacePermission;

        public CardPermissionService(
            IUnitOfWork unitOfWork,
            IBoardPermissionService boardPermission,
            IWorkspacePermissionService workspacePermission)
        {
            _cardRepository = unitOfWork.GetRepository<Card, Guid>();
            _assigneeRepository = unitOfWork.GetRepository<CardAssignee, Guid>();
            _listRepository = unitOfWork.GetRepository<List, Guid>();
            _boardRepository = unitOfWork.GetRepository<Board, Guid>();
            _boardPermission = boardPermission;
            _workspacePermission = workspacePermission;
        }

        private async Task<Card> GetCardOrThrow(Guid cardId)
        {
            var card = await _cardRepository.GetByIdAsync(cardId);
            if (card == null)
                throw new NotFoundException("Card không tồn tại.");

            return card;
        }

        private async Task<(Guid boardId, Guid workspaceId)> GetBoardInfo(Guid cardId)
        {
            var card = await GetCardOrThrow(cardId);
            var list = await _listRepository.GetByIdAsync(card.ListId)
                ?? throw new NotFoundException("List chứa Card không tồn tại.");

            var board = await _boardRepository.GetByIdAsync(list.BoardId)
                ?? throw new NotFoundException("Board chứa List không tồn tại.");

            return (list.BoardId, board.WorkspaceId);
        }

        private async Task<bool> IsAssignee(Guid cardId, Guid userId)
        {
            return await _assigneeRepository.AnyAsync(
                x => x.CardId == cardId && x.UserId == userId
            );
        }

        private async Task<bool> IsWorkspaceAdminOrOwner(Guid workspaceId, Guid userId)
        {
            var role = await _workspacePermission.GetRoleAsync(workspaceId, userId);
            return role is WorkSpaceRole.Admin or WorkSpaceRole.Owner;
        }

        private async Task<bool> IsBoardEditorOrOwner(Guid boardId, Guid userId)
        {
            var role = await _boardPermission.GetRoleAsync(boardId, userId);
            return role is BoardRole.Editor or BoardRole.Owner;
        }

        public async Task EnsureCanViewAsync(Guid cardId, Guid userId)
        {
            var (boardId, workspaceId) = await GetBoardInfo(cardId);

            if (await IsWorkspaceAdminOrOwner(workspaceId, userId))
                return;

            await _boardPermission.EnsureViewerAsync(boardId, userId);
        }

        public async Task EnsureCanEditAsync(Guid cardId, Guid userId)
        {
            var (boardId, workspaceId) = await GetBoardInfo(cardId);

            if (await IsWorkspaceAdminOrOwner(workspaceId, userId))
                return;

            if (await IsBoardEditorOrOwner(boardId, userId))
                return;

            if (await IsAssignee(cardId, userId))
                return;

            throw new ForbiddenAccessException("Bạn không có quyền chỉnh sửa Card này.");
        }

        public async Task EnsureCanAssignAsync(Guid cardId, Guid userId)
        {
            var (boardId, workspaceId) = await GetBoardInfo(cardId);

            if (await IsWorkspaceAdminOrOwner(workspaceId, userId))
                return;

            if (await IsBoardEditorOrOwner(boardId, userId))
                return;

            if (await IsAssignee(cardId, userId))
                throw new ForbiddenAccessException("Assignee không có quyền gán người cho Card.");

            throw new ForbiddenAccessException("Bạn không có quyền gán người vào Card.");
        }

        public async Task EnsureCanCommentAsync(Guid cardId, Guid userId)
        {
            await EnsureCanViewAsync(cardId, userId);
        }

        public async Task EnsureCanDeleteAsync(Guid cardId, Guid userId)
        {
            var (boardId, workspaceId) = await GetBoardInfo(cardId);

            if (await IsWorkspaceAdminOrOwner(workspaceId, userId))
                return;

            if (await IsBoardEditorOrOwner(boardId, userId))
                return;

            if (await IsAssignee(cardId, userId))
                throw new ForbiddenAccessException("Assignee không có quyền xoá Card.");

            throw new ForbiddenAccessException("Bạn không có quyền xoá Card.");
        }
    }
}
