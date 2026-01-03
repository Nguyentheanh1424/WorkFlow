using AutoMapper;
using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Constants.EventNames;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.SubTasks.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.SubTasks.Commands
{
    public record CreateSubTaskCommand(
        Guid TaskId,
        string Title
    ) : IRequest<Result<SubTaskDto>>;

    public class CreateSubTaskCommandValidator : AbstractValidator<CreateSubTaskCommand>
    {
        public CreateSubTaskCommandValidator()
        {
            RuleFor(x => x.TaskId).NotEmpty();
            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(200);
        }
    }

    public class CreateSubTaskCommandHandler
        : IRequestHandler<CreateSubTaskCommand, Result<SubTaskDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;
        private readonly IMapper _mapper;

        public CreateSubTaskCommandHandler(
            IUnitOfWork unitOfWork,
            IBoardPermissionService permission,
            ICurrentUserService currentUser,
            IRealtimeService realtime,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _permission = permission;
            _currentUser = currentUser;
            _realtime = realtime;
            _mapper = mapper;
        }

        public async Task<Result<SubTaskDto>> Handle(CreateSubTaskCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId is null)
                return Result<SubTaskDto>.Failure("Không xác định người dùng.");

            var userId = _currentUser.UserId.Value;

            var taskRepo = _unitOfWork.GetRepository<Domain.Entities.Task, Guid>();
            var subTaskRepo = _unitOfWork.GetRepository<SubTask, Guid>();
            var cardRepo = _unitOfWork.GetRepository<Card, Guid>();
            var listRepo = _unitOfWork.GetRepository<List, Guid>();
            var boardRepo = _unitOfWork.GetRepository<Board, Guid>();

            var task = await taskRepo.GetByIdAsync(request.TaskId);
            if (task == null)
                return Result<SubTaskDto>.Failure("Task không tồn tại.");

            var card = await cardRepo.GetByIdAsync(task.CardId)
                ?? throw new NotFoundException("Card không tồn tại.");

            var list = await listRepo.GetByIdAsync(card.ListId)
                ?? throw new NotFoundException("List không tồn tại.");

            var board = await boardRepo.GetByIdAsync(list.BoardId)
                ?? throw new NotFoundException("Board không tồn tại.");

            await _permission.EnsureEditorAsync(board.Id, userId);

            var existing = await subTaskRepo.FindAsync(x => x.TaskId == task.Id);
            var position = existing.Count;

            var subTask = SubTask.Create(task.Id, request.Title, position);

            await subTaskRepo.AddAsync(subTask);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<SubTaskDto>(subTask);

            await _realtime.SendToBoardAsync(board.Id, "BoardNotification", new { Action = TaskEvents.SubTaskCreated, Data = dto });

            return Result<SubTaskDto>.Success(dto);
        }
    }
}
