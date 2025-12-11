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
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.SubTasks.Commands
{
    public record UpdateSubTaskStatusCommand(
        Guid SubTaskId,
        JobStatus Status
    ) : IRequest<Result>;

    public class UpdateSubTaskStatusCommandValidator : AbstractValidator<UpdateSubTaskStatusCommand>
    {
        public UpdateSubTaskStatusCommandValidator()
        {
            RuleFor(x => x.SubTaskId).NotEmpty();

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Trạng thái không hợp lệ.");
        }
    }

    public class UpdateSubTaskStatusCommandHandler
        : IRequestHandler<UpdateSubTaskStatusCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;

        public UpdateSubTaskStatusCommandHandler(
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

        public async Task<Result> Handle(UpdateSubTaskStatusCommand request, CancellationToken cancellationToken)
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

            subTask.UpdateStatus(request.Status);

            await subTaskRepo.UpdateAsync(subTask);
            await _unitOfWork.SaveChangesAsync();

            await _realtime.SendToBoardAsync(board.Id, TaskEvents.SubTaskUpdated, new
            {
                SubTaskId = subTask.Id,
                Status = request.Status
            });

            return Result.Success();
        }
    }
}
