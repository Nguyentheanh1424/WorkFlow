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

namespace WorkFlow.Application.Features.Tasks.Commands
{
    public record DeleteTaskCommand(Guid TaskId) : IRequest<Result>;

    public class DeleteTaskCommandValidator : AbstractValidator<DeleteTaskCommand>
    {
        public DeleteTaskCommandValidator()
        {
            RuleFor(x => x.TaskId).NotEmpty();
        }
    }

    public class DeleteTaskCommandHandler
        : IRequestHandler<DeleteTaskCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;

        public DeleteTaskCommandHandler(
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

        public async Task<Result> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId is null)
                return Result.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var taskRepo = _unitOfWork.GetRepository<Domain.Entities.Task, Guid>();
            var subTaskRepo = _unitOfWork.GetRepository<SubTask, Guid>();
            var subTaskAssigneeRepo = _unitOfWork.GetRepository<SubTaskAssignee, Guid>();
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

            var subTasks = await subTaskRepo.FindAsync(x => x.TaskId == task.Id);

            foreach (var sub in subTasks)
            {
                var assignees = await subTaskAssigneeRepo.FindAsync(a => a.SubTaskId == sub.Id);
                foreach (var a in assignees)
                    await subTaskAssigneeRepo.DeleteAsync(a);

                await subTaskRepo.DeleteAsync(sub);
            }

            await taskRepo.DeleteAsync(task);

            await _unitOfWork.SaveChangesAsync();

            await _realtime.SendToBoardAsync(board.Id, TaskEvents.Deleted, new
            {
                TaskId = task.Id,
                CardId = card.Id
            });

            return Result.Success();
        }
    }
}
