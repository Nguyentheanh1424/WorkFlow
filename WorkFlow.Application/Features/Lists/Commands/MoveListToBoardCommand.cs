using AutoMapper;
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
using WorkFlow.Application.Features.Lists.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Lists.Commands
{
    public record MoveListToBoardCommand(Guid ListId, Guid TargetBoardId)
        : IRequest<Result<ListDto>>;

    public class MoveListToBoardCommandValidator : AbstractValidator<MoveListToBoardCommand>
    {
        public MoveListToBoardCommandValidator()
        {
            RuleFor(x => x.ListId)
                .NotEmpty().WithMessage("ListId không được để trống.");

            RuleFor(x => x.TargetBoardId)
                .NotEmpty().WithMessage("TargetBoardId không được để trống.");
        }
    }

    public class MoveListToBoardCommandHandler
        : IRequestHandler<MoveListToBoardCommand, Result<ListDto>>
    {
        private readonly IRepository<List, Guid> _listRepository;
        private readonly IRepository<Card, Guid> _cardRepository;
        private readonly IRepository<Board, Guid> _boardRepository;
        private readonly IRepository<BoardMember, Guid> _boardMemberRepository;
        private readonly IRepository<CardAssignee, Guid> _cardAssigneeRepository;
        private readonly IRepository<Domain.Entities.Task, Guid> _taskRepository;
        private readonly IRepository<SubTask, Guid> _subTaskRepository;
        private readonly IRepository<SubTaskAssignee, Guid> _subTaskAssigneeRepository;

        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MoveListToBoardCommandHandler(
            IBoardPermissionService permission,
            ICurrentUserService currentUser,
            IRealtimeService realtime,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _listRepository = unitOfWork.GetRepository<List, Guid>();
            _boardRepository = unitOfWork.GetRepository<Board, Guid>();
            _cardRepository = unitOfWork.GetRepository<Card, Guid>();
            _boardMemberRepository = unitOfWork.GetRepository<BoardMember, Guid>();
            _cardAssigneeRepository = unitOfWork.GetRepository<CardAssignee, Guid>();
            _taskRepository = unitOfWork.GetRepository<Domain.Entities.Task, Guid>();
            _subTaskRepository = unitOfWork.GetRepository<SubTask, Guid>();
            _subTaskAssigneeRepository = unitOfWork.GetRepository<SubTaskAssignee, Guid>();

            _permission = permission;
            _currentUser = currentUser;
            _realtime = realtime;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<ListDto>> Handle(MoveListToBoardCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<ListDto>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var list = await _listRepository.GetByIdAsync(request.ListId);
            if (list == null)
                return Result<ListDto>.Failure("List không tồn tại.");

            var sourceBoardId = list.BoardId;

            var sourceBoard = await _boardRepository.GetByIdAsync(sourceBoardId)
                ?? throw new NotFoundException("Board nguồn không tồn tại.");

            var targetBoard = await _boardRepository.GetByIdAsync(request.TargetBoardId)
                ?? throw new NotFoundException("Board đích không tồn tại.");

            if (sourceBoard.Id == targetBoard.Id)
                return Result<ListDto>.Failure("Board nguồn và board đích không được trùng nhau.");

            await _permission.EnsureEditorAsync(sourceBoard.Id, userId);
            await _permission.EnsureEditorAsync(targetBoard.Id, userId);

            var sourceLists = await _listRepository.FindAsync(l => l.BoardId == sourceBoard.Id);
            var orderedSource = sourceLists.OrderBy(l => l.Position).ToList();

            orderedSource.RemoveAll(l => l.Id == list.Id);

            for (int i = 0; i < orderedSource.Count; i++)
            {
                orderedSource[i].MoveTo(i);
            }

            var targetLists = await _listRepository.FindAsync(l => l.BoardId == targetBoard.Id);
            var newPosition = targetLists.Count == 0 ? 0 : targetLists.Max(l => l.Position) + 1;

            list.MoveToBoard(targetBoard.Id);
            list.MoveTo(newPosition);

            var cards = await _cardRepository.FindAsync(c => c.ListId == list.Id);
            var cardIds = cards.Select(c => c.Id).ToList();

            var targetBoardMembers = await _boardMemberRepository
                .FindAsync(m => m.BoardId == targetBoard.Id);

            var memberIds = targetBoardMembers
                .Select(m => m.UserId)
                .ToHashSet();

            if (cardIds.Count > 0)
            {
                var cardAssignees = await _cardAssigneeRepository
                    .FindAsync(a => cardIds.Contains(a.CardId));

                foreach (var assignee in cardAssignees)
                {
                    if (!memberIds.Contains(assignee.UserId))
                    {
                        await _cardAssigneeRepository.DeleteAsync(assignee);
                    }
                }
            }

            var tasks = cardIds.Count == 0
                ? new List<Domain.Entities.Task>()
                : await _taskRepository.FindAsync(t => cardIds.Contains(t.CardId));

            var taskIds = tasks.Select(t => t.Id).ToList();

            if (taskIds.Count > 0)
            {
                var subTasks = await _subTaskRepository.FindAsync(st => taskIds.Contains(st.TaskId));
                var subTaskIds = subTasks.Select(st => st.Id).ToList();

                if (subTaskIds.Count > 0)
                {
                    var subAssignees = await _subTaskAssigneeRepository
                        .FindAsync(a => subTaskIds.Contains(a.SubTaskId));

                    foreach (var sa in subAssignees)
                    {
                        if (!memberIds.Contains(sa.UserId))
                        {
                            await _subTaskAssigneeRepository.DeleteAsync(sa);
                        }
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<ListDto>(list);

            await _realtime.SendToBoardAsync(sourceBoard.Id, ListEvents.Deleted, dto);

            await _realtime.SendToBoardAsync(targetBoard.Id, ListEvents.Created, dto);

            return Result<ListDto>.Success(dto);
        }
    }
}
