using AutoMapper;
using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Constants.EventNames;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.Tasks.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Tasks.Commands
{
    public record CreateTaskCommand(
        Guid CardId,
        string Title
    ) : IRequest<Result<TaskDto>>;

    public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
    {
        public CreateTaskCommandValidator()
        {
            RuleFor(x => x.CardId).NotEmpty();
            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(200);
        }
    }

    public class CreateTaskCommandHandler
        : IRequestHandler<CreateTaskCommand, Result<TaskDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;
        private readonly IMapper _mapper;

        public CreateTaskCommandHandler(
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

        public async Task<Result<TaskDto>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<TaskDto>.Failure("Không xác định được người dùng.");
            var userId = _currentUser.UserId.Value;

            var taskRepo = _unitOfWork.GetRepository<Domain.Entities.Task, Guid>();
            var cardRepo = _unitOfWork.GetRepository<Card, Guid>();
            var listRepo = _unitOfWork.GetRepository<List, Guid>();
            var boardRepo = _unitOfWork.GetRepository<Board, Guid>();

            var card = await cardRepo.GetByIdAsync(request.CardId);
            if (card == null)
                return Result<TaskDto>.Failure("Card không tồn tại.");

            var list = await listRepo.GetByIdAsync(card.ListId)
                ?? throw new NotFoundException("List không tồn tại.");
            var board = await boardRepo.GetByIdAsync(list.BoardId)
                ?? throw new NotFoundException("Board không tồn tại.");

            await _permission.EnsureEditorAsync(board.Id, userId);

            var tasks = await taskRepo.FindAsync(t => t.CardId == card.Id);
            var position = tasks.Count;

            var task = Domain.Entities.Task.Create(card.Id, request.Title, position);

            await taskRepo.AddAsync(task);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<TaskDto>(task);

            await _realtime.SendToBoardAsync(board.Id, TaskEvents.Created, dto);

            return Result<TaskDto>.Success(dto);
        }
    }
}
