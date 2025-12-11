using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Constants.EventNames;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.SubTasks.Commands
{
    public record UpdateSubTaskDatesCommand(
        Guid SubTaskId,
        DateTime? DueDate,
        bool ReminderEnabled,
        int? ReminderBeforeMinutes
    ) : IRequest<Result>;

    public class UpdateSubTaskDatesCommandValidator : AbstractValidator<UpdateSubTaskDatesCommand>
    {
        public UpdateSubTaskDatesCommandValidator()
        {
            RuleFor(x => x.SubTaskId).NotEmpty();

            RuleFor(x => x.DueDate)
                .Must(date => !date.HasValue || date.Value > DateTime.MinValue)
                .WithMessage("DueDate không hợp lệ.");

            RuleFor(x => x)
                .Must(cmd => !cmd.ReminderEnabled || cmd.ReminderBeforeMinutes.HasValue)
                .WithMessage("ReminderBeforeMinutes phải có giá trị khi ReminderEnabled = true.");

            RuleFor(x => x.ReminderBeforeMinutes)
                .GreaterThan(0)
                .When(x => x.ReminderEnabled && x.ReminderBeforeMinutes.HasValue)
                .WithMessage("ReminderBeforeMinutes phải > 0.");
        }
    }

    public class UpdateSubTaskDatesCommandHandler
        : IRequestHandler<UpdateSubTaskDatesCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;

        public UpdateSubTaskDatesCommandHandler(
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

        public async Task<Result> Handle(UpdateSubTaskDatesCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId is null)
                return Result.Failure("Không xác định người dùng.");

            var userId = _currentUser.UserId.Value;

            var subTaskRepo = _unitOfWork.GetRepository<SubTask, Guid>();
            var taskRepo = _unitOfWork.GetRepository<Domain.Entities.Task, Guid>();
            var cardRepo = _unitOfWork.GetRepository<Card, Guid>();
            var listRepo = _unitOfWork.GetRepository<List, Guid>();
            var boardRepo = _unitOfWork.GetRepository<Board, Guid>();

            var subTask = await subTaskRepo.GetByIdAsync(request.SubTaskId);
            if (subTask == null)
                return Result.Failure("SubTask không tồn tại.");

            var task = await taskRepo.GetByIdAsync(subTask.TaskId)
                ?? throw new NotFoundException("Task không tồn tại.");

            var card = await cardRepo.GetByIdAsync(task.CardId)
                ?? throw new NotFoundException("Card không tồn tại.");

            var list = await listRepo.GetByIdAsync(card.ListId)
                ?? throw new NotFoundException("List không tồn tại.");

            var board = await boardRepo.GetByIdAsync(list.BoardId)
                ?? throw new NotFoundException("Board không tồn tại.");

            await _permission.EnsureEditorAsync(board.Id, userId);

            subTask.SetDueDate(request.DueDate);

            if (request.ReminderEnabled)
                subTask.EnableReminder(request.ReminderBeforeMinutes!.Value);
            else
                subTask.DisableReminder();

            await subTaskRepo.UpdateAsync(subTask);
            await _unitOfWork.SaveChangesAsync();

            await _realtime.SendToBoardAsync(board.Id, TaskEvents.SubTaskUpdated, new
            {
                SubTaskId = subTask.Id,
                DueDate = request.DueDate,
                ReminderEnabled = request.ReminderEnabled,
                ReminderBeforeMinutes = request.ReminderBeforeMinutes
            });

            return Result.Success();
        }
    }
}
