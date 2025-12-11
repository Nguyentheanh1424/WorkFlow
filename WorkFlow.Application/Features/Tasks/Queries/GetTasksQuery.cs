using AutoMapper;
using MediatR;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.SubTasks.Dtos;
using WorkFlow.Application.Features.Tasks.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.Tasks.Queries
{
    public record GetTasksQuery(Guid CardId)
        : IRequest<Result<List<TaskDto>>>;

    public class GetTasksQueryHandler
        : IRequestHandler<GetTasksQuery, Result<List<TaskDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public GetTasksQueryHandler(
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

        public async Task<Result<List<TaskDto>>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<List<TaskDto>>.Failure("Không xác định người dùng.");

            var userId = _currentUser.UserId.Value;

            var taskRepo = _unitOfWork.GetRepository<WorkFlow.Domain.Entities.Task, Guid>();
            var subTaskRepo = _unitOfWork.GetRepository<SubTask, Guid>();
            var cardRepo = _unitOfWork.GetRepository<Card, Guid>();
            var listRepo = _unitOfWork.GetRepository<List, Guid>();
            var boardRepo = _unitOfWork.GetRepository<Board, Guid>();

            var card = await cardRepo.GetByIdAsync(request.CardId);
            if (card == null)
                return Result<List<TaskDto>>.Failure("Card không tồn tại.");

            var list = await listRepo.GetByIdAsync(card.ListId)
                ?? throw new NotFoundException("List không tồn tại.");

            var board = await boardRepo.GetByIdAsync(list.BoardId)
                ?? throw new NotFoundException("Board không tồn tại.");

            await _permission.EnsureViewerAsync(board.Id, userId);

            var tasks = await taskRepo.FindAsync(t => t.CardId == card.Id);

            var dtos = new List<TaskDto>();

            foreach (var task in tasks.OrderBy(t => t.Position))
            {
                var dto = _mapper.Map<TaskDto>(task);

                var subTasks = await subTaskRepo.FindAsync(st => st.TaskId == task.Id);
                dto.SubTasks = subTasks
                    .OrderBy(x => x.Position)
                    .Select(x => _mapper.Map<SubTaskDto>(x))
                    .ToList();

                dto.TotalSubTasks = dto.SubTasks.Count;
                dto.CompletedSubTasks = dto.SubTasks.Count(x => x.Status == JobStatus.Done);
                dto.Progress = dto.TotalSubTasks == 0
                    ? 0
                    : (double)dto.CompletedSubTasks / dto.TotalSubTasks;

                dtos.Add(dto);
            }

            return Result<List<TaskDto>>.Success(dtos);
        }
    }
}
