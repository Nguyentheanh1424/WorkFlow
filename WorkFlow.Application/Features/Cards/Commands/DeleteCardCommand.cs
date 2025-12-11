using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Constants.EventNames;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Cards.Commands
{
    public record DeleteCardCommand(Guid CardId)
        : IRequest<Result>;

    public class DeleteCardCommandValidator : AbstractValidator<DeleteCardCommand>
    {
        public DeleteCardCommandValidator()
        {
            RuleFor(x => x.CardId).NotEmpty();
        }
    }

    public class DeleteCardCommandHandler
        : IRequestHandler<DeleteCardCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;

        public DeleteCardCommandHandler(
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

        public async Task<Result> Handle(DeleteCardCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var cardRepo = _unitOfWork.GetRepository<Card, Guid>();
            var listRepo = _unitOfWork.GetRepository<List, Guid>();
            var boardRepo = _unitOfWork.GetRepository<Board, Guid>();
            var taskRepo = _unitOfWork.GetRepository<Domain.Entities.Task, Guid>();
            var subTaskRepo = _unitOfWork.GetRepository<SubTask, Guid>();
            var subTaskAssigneeRepo = _unitOfWork.GetRepository<SubTaskAssignee, Guid>();
            var assigneeRepo = _unitOfWork.GetRepository<CardAssignee, Guid>();
            var attachmentRepo = _unitOfWork.GetRepository<Attachment, Guid>();

            var card = await cardRepo.GetByIdAsync(request.CardId);
            if (card == null)
                return Result.Failure("Card không tồn tại.");

            var list = await listRepo.GetByIdAsync(card.ListId)
                ?? throw new NotFoundException("List không tồn tại.");

            var board = await boardRepo.GetByIdAsync(list.BoardId)
                ?? throw new NotFoundException("Board không tồn tại.");

            await _permission.EnsureEditorAsync(board.Id, userId);

            var attachments = await attachmentRepo.FindAsync(a => a.CardId == card.Id);
            foreach (var att in attachments)
                await attachmentRepo.DeleteAsync(att);

            var assignees = await assigneeRepo.FindAsync(a => a.CardId == card.Id);
            foreach (var a in assignees)
                await assigneeRepo.DeleteAsync(a);

            var tasks = await taskRepo.FindAsync(t => t.CardId == card.Id);
            foreach (var task in tasks)
            {
                var subs = await subTaskRepo.FindAsync(s => s.TaskId == task.Id);
                foreach (var sub in subs)
                {
                    var subAssignees = await subTaskAssigneeRepo.FindAsync(sa => sa.SubTaskId == sub.Id);
                    foreach (var sa in subAssignees)
                        await subTaskAssigneeRepo.DeleteAsync(sa);

                    await subTaskRepo.DeleteAsync(sub);
                }

                await taskRepo.DeleteAsync(task);
            }

            await cardRepo.DeleteAsync(card);

            await _unitOfWork.SaveChangesAsync();

            await _realtime.SendToBoardAsync(board.Id, CardEvents.Deleted, new
            {
                CardId = card.Id,
                ListId = list.Id
            });

            return Result.Success();
        }
    }
}
