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
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Tasks.Commands
{
    public record MoveTaskCommand(
        Guid TaskId,
        int NewPosition
    ) : IRequest<Result>;

    public class MoveTaskCommandValidator : AbstractValidator<MoveTaskCommand>
    {
        public MoveTaskCommandValidator()
        {
            RuleFor(x => x.TaskId).NotEmpty();
            RuleFor(x => x.NewPosition).GreaterThanOrEqualTo(0);
        }
    }

    public class MoveTaskCommandHandler
        : IRequestHandler<MoveTaskCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;

        public MoveTaskCommandHandler(
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

        public async Task<Result> Handle(MoveTaskCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId is null)
                return Result.Failure("Không xác định người dùng.");

            var userId = _currentUser.UserId.Value;

            var taskRepo = _unitOfWork.GetRepository<Domain.Entities.Task, Guid>();
            var cardRepo = _unitOfWork.GetRepository<Card, Guid>();
            var listRepo = _unitOfWork.GetRepository<List, Guid>();
            var boardRepo = _unitOfWork.GetRepository<Board, Guid>();

            var task = await taskRepo.GetByIdAsync(request.TaskId);
            if (task == null)
                return Result.Failure("Task không tồn tại.");

            var card = await cardRepo.GetByIdAsync(task.CardId)
                ?? throw new NotFoundException("Card không tồn tại.");

            var list = await listRepo.GetByIdAsync(card.ListId)
                ?? throw new NotFoundException("List không tồn tại.");

            var board = await boardRepo.GetByIdAsync(list.BoardId)
                ?? throw new NotFoundException("Board không tồn tại.");

            await _permission.EnsureEditorAsync(board.Id, userId);

            var tasks = await taskRepo.FindAsync(t => t.CardId == card.Id);

            var ordered = tasks.OrderBy(t => t.Position).ToList();

            ordered.Remove(task);

            var newPos = Math.Min(request.NewPosition, ordered.Count);
            ordered.Insert(newPos, task);

            int pos = 0;
            foreach (var t in ordered)
            {
                t.MoveTo(pos++);
                await taskRepo.UpdateAsync(t);
            }

            await _unitOfWork.SaveChangesAsync();

            await _realtime.SendToBoardAsync(board.Id, TaskEvents.Moved, new
            {
                TaskId = task.Id,
                NewPosition = newPos,
                CardId = card.Id
            });

            return Result.Success();
        }
    }
}
