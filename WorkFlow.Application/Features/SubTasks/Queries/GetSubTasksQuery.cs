using AutoMapper;
using MediatR;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.SubTasks.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.SubTasks.Queries
{
    public record GetSubTasksQuery(Guid TaskId)
        : IRequest<Result<List<SubTaskDto>>>;

    public class GetSubTasksQueryHandler
        : IRequestHandler<GetSubTasksQuery, Result<List<SubTaskDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public GetSubTasksQueryHandler(
            IUnitOfWork unitOfWork,
            IBoardPermissionService permission,
            ICurrentUserService currentUser,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _permission = permission;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<Result<List<SubTaskDto>>> Handle(
            GetSubTasksQuery request,
            CancellationToken cancellationToken)
        {
            if (_currentUser.UserId is null)
                return Result<List<SubTaskDto>>.Failure("Không xác định người dùng.");

            var userId = _currentUser.UserId.Value;

            var subTaskRepo = _unitOfWork.GetRepository<SubTask, Guid>();
            var taskRepo = _unitOfWork.GetRepository<Domain.Entities.Task, Guid>();
            var cardRepo = _unitOfWork.GetRepository<Card, Guid>();
            var listRepo = _unitOfWork.GetRepository<List, Guid>();
            var boardRepo = _unitOfWork.GetRepository<Board, Guid>();

            var task = await taskRepo.GetByIdAsync(request.TaskId);
            if (task == null)
                return Result<List<SubTaskDto>>.Failure("Task không tồn tại.");

            var card = await cardRepo.GetByIdAsync(task.CardId)
                ?? throw new NotFoundException("Card không tồn tại.");

            var list = await listRepo.GetByIdAsync(card.ListId)
                ?? throw new NotFoundException("List không tồn tại.");

            var board = await boardRepo.GetByIdAsync(list.BoardId)
                ?? throw new NotFoundException("Board không tồn tại.");

            await _permission.EnsureViewerAsync(board.Id, userId);

            var subs = await subTaskRepo.FindAsync(x => x.TaskId == task.Id);

            var dtos = subs
                .OrderBy(x => x.Position)
                .Select(x => _mapper.Map<SubTaskDto>(x))
                .ToList();

            return Result<List<SubTaskDto>>.Success(dtos);
        }
    }
}
