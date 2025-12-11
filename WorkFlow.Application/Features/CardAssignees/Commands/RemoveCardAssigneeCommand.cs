using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Application.Common.Constants.EventNames;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.CardAssignees.Commands
{
    public record RemoveCardAssigneeCommand(Guid CardId, Guid UserId)
        : IRequest<Result>;

    public class RemoveCardAssigneeCommandValidator : AbstractValidator<RemoveCardAssigneeCommand>
    {
        public RemoveCardAssigneeCommandValidator()
        {
            RuleFor(x => x.CardId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
        }
    }

    public class RemoveCardAssigneeCommandHandler
        : IRequestHandler<RemoveCardAssigneeCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;

        public RemoveCardAssigneeCommandHandler(
            IUnitOfWork unitOfWork,
            IBoardPermissionService permission,
            ICurrentUserService currentUser,
            IRealtimeService realtime)
        {
            _unitOfWork = unitOfWork;
            _permission = permission;
            _currentUser = currentUser;
            _realtime = realtime;
        }

        public async Task<Result> Handle(RemoveCardAssigneeCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result.Failure("Không xác định được người dùng.");

            var currentUserId = _currentUser.UserId.Value;

            var cardRepo = _unitOfWork.GetRepository<Card, Guid>();
            var listRepo = _unitOfWork.GetRepository<List, Guid>();
            var boardRepo = _unitOfWork.GetRepository<Board, Guid>();
            var assigneeRepo = _unitOfWork.GetRepository<CardAssignee, Guid>();

            var card = await cardRepo.GetByIdAsync(request.CardId);
            if (card == null)
                return Result.Failure("Card không tồn tại.");

            var list = await listRepo.GetByIdAsync(card.ListId)
                ?? throw new NotFoundException("List không tồn tại.");

            var board = await boardRepo.GetByIdAsync(list.BoardId)
                ?? throw new NotFoundException("Board không tồn tại.");

            await _permission.EnsureEditorAsync(board.Id, currentUserId);

            var assignee = await assigneeRepo.FirstOrDefaultAsync(x =>
                x.CardId == card.Id && x.UserId == request.UserId);

            if (assignee == null)
                return Result.Failure("User không phải assignee của card.");

            await assigneeRepo.DeleteAsync(assignee);
            await _unitOfWork.SaveChangesAsync();

            await _realtime.SendToBoardAsync(board.Id, CardEvents.AssigneeRemoved, new
            {
                CardId = card.Id,
                RemovedUserId = request.UserId
            });

            return Result.Success();
        }
    }
}
