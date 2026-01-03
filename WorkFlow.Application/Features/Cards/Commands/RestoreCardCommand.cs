using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkFlow.Application.Common.Constants.EventNames;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Cards.Commands
{
    public record RestoreCardCommand(Guid CardId)
        : IRequest<Result>;

    public class RestoreCardCommandValidator
        : AbstractValidator<RestoreCardCommand>
    {
        public RestoreCardCommandValidator()
        {
            RuleFor(x => x.CardId).NotEmpty();
        }
    }

    public class RestoreCardCommandHandler
    : IRequestHandler<RestoreCardCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;

        public RestoreCardCommandHandler(
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

        public async Task<Result> Handle(
            RestoreCardCommand request,
            CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var cardRepo = _unitOfWork.GetRepository<Card, Guid>();
            var listRepo = _unitOfWork.GetRepository<List, Guid>();
            var boardRepo = _unitOfWork.GetRepository<Board, Guid>();
            var taskRepo = _unitOfWork.GetRepository<Domain.Entities.Task, Guid>();
            var subTaskRepo = _unitOfWork.GetRepository<SubTask, Guid>();

            var card = await cardRepo
                .GetAll()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken);

            if (card == null)
                return Result.Failure("Card không tồn tại.");

            if (!card.IsDeleted)
                return Result.Failure("Card chưa bị xoá.");

            var list = await listRepo.GetByIdAsync(card.ListId)
                ?? throw new NotFoundException("List không tồn tại.");

            var board = await boardRepo.GetByIdAsync(list.BoardId)
                ?? throw new NotFoundException("Board không tồn tại.");

            await _permission.EnsureEditorAsync(board.Id, userId);

            if (card.IsValidRedo().IsValid == false)
                return Result.Failure("Card không thể khôi phục do: " + card.IsValidRedo().Message);

            card.RedoDeleted();

            var tasks = await taskRepo
                .GetAll()
                .IgnoreQueryFilters()
                .Where(t => t.CardId == card.Id)
                .ToListAsync(cancellationToken);

            foreach (var task in tasks)
            {
                if (task.IsDeleted)
                    task.RedoDeleted();

                var subTasks = await subTaskRepo
                    .GetAll()
                    .IgnoreQueryFilters()
                    .Where(s => s.TaskId == task.Id)
                    .ToListAsync(cancellationToken);

                foreach (var sub in subTasks)
                {
                    if (sub.IsDeleted)
                        sub.RedoDeleted();
                }
            }

            await _unitOfWork.SaveChangesAsync();

            await _realtime.SendToBoardAsync(
                board.Id,
                "BoardNotification",
                new
                {
                    Action = CardEvents.Restored,
                    CardId = card.Id,
                    ListId = list.Id
                });

            return Result.Success();
        }
    }
}