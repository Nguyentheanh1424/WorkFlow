using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Entities;
using Task = System.Threading.Tasks.Task;

namespace WorkFlow.Application.Common.Services
{
    public class CardPermissionService : ICardPermissionService
    {
        private readonly IRepository<Card, Guid> _cardRepository;
        private readonly IRepository<CardAssignee, Guid> _assigneeRepository;
        private readonly IRepository<List, Guid> _listRepository;
        private readonly IBoardPermissionService _boardPermission;

        public CardPermissionService(
            IUnitOfWork unitOfWork,
            IBoardPermissionService boardPermission)
        {
            _cardRepository = unitOfWork.GetRepository<Card, Guid>();
            _assigneeRepository = unitOfWork.GetRepository<CardAssignee, Guid>();
            _listRepository = unitOfWork.GetRepository<List, Guid>();
            _boardPermission = boardPermission;
        }

        private async Task<Card> GetCardOrThrow(Guid cardId)
        {
            var card = await _cardRepository.GetByIdAsync(cardId);
            if (card == null)
                throw new NotFoundException("Card không tồn tại.");

            return card;
        }

        private async Task<Guid> GetBoardIdFromCardAsync(Guid cardId)
        {
            var card = await GetCardOrThrow(cardId);
            var list = await _listRepository.GetByIdAsync(card.ListId);

            if (list == null)
                throw new NotFoundException("List chứa Card không tồn tại.");

            return list.BoardId;
        }

        private async Task<bool> IsAssignee(Guid cardId, Guid userId)
        {
            return await _assigneeRepository.AnyAsync(
                x => x.CardId == cardId && x.UserId == userId
            );
        }

        public async Task EnsureCanViewAsync(Guid cardId, Guid userId)
        {
            var boardId = await GetBoardIdFromCardAsync(cardId);

            await _boardPermission.EnsureViewerAsync(boardId, userId);
        }

        public async Task EnsureCanEditAsync(Guid cardId, Guid userId)
        {
            var boardId = await GetBoardIdFromCardAsync(cardId);

            try
            {
                await _boardPermission.EnsureEditorAsync(boardId, userId);
                return;
            }
            catch
            {
                // fallthrough → check assignee
            }

            if (await IsAssignee(cardId, userId))
                return;

            throw new ForbiddenAccessException("Bạn không có quyền chỉnh sửa Card này.");
        }

        public async Task EnsureCanAssignAsync(Guid cardId, Guid userId)
        {
            var boardId = await GetBoardIdFromCardAsync(cardId);

            await _boardPermission.EnsureEditorAsync(boardId, userId);
        }

        public async Task EnsureCanCommentAsync(Guid cardId, Guid userId)
        {
            var boardId = await GetBoardIdFromCardAsync(cardId);

            await _boardPermission.EnsureViewerAsync(boardId, userId);
        }

        public async Task EnsureCanDeleteAsync(Guid cardId, Guid userId)
        {
            var boardId = await GetBoardIdFromCardAsync(cardId);

            await _boardPermission.EnsureEditorAsync(boardId, userId);
        }
    }

}
