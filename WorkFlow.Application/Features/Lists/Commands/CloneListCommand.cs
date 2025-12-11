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
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.Lists.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Lists.Commands
{
    public record CloneListCommand(Guid ListId, bool CopyCards = true)
        : IRequest<Result<ListDto>>;

    public class CloneListCommandValidator : AbstractValidator<CloneListCommand>
    {
        public CloneListCommandValidator()
        {
            RuleFor(x => x.ListId)
                .NotEmpty().WithMessage("ListId không được để trống.");
        }
    }

    public class CloneListCommandHandler
        : IRequestHandler<CloneListCommand, Result<ListDto>>
    {
        private readonly IRepository<List, Guid> _listRepository;
        private readonly IRepository<Card, Guid> _cardRepository;
        private readonly IRepository<Domain.Entities.Task, Guid> _taskRepository;
        private readonly IRepository<SubTask, Guid> _subTaskRepository;
        private readonly IRepository<CardAssignee, Guid> _cardAssigneeRepository;
        private readonly IRepository<SubTaskAssignee, Guid> _subTaskAssigneeRepository;
        private readonly IRepository<Board, Guid> _boardRepository;
        private readonly IRepository<BoardMember, Guid> _boardMemberRepository;

        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CloneListCommandHandler(
            IBoardPermissionService permission,
            ICurrentUserService currentUser,
            IRealtimeService realtime,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _listRepository = unitOfWork.GetRepository<List, Guid>();
            _boardRepository = unitOfWork.GetRepository<Board, Guid>();
            _taskRepository = unitOfWork.GetRepository<Domain.Entities.Task, Guid>();
            _subTaskRepository = unitOfWork.GetRepository<SubTask, Guid>();
            _cardAssigneeRepository = unitOfWork.GetRepository<CardAssignee, Guid>();
            _subTaskAssigneeRepository = unitOfWork.GetRepository<SubTaskAssignee, Guid>();
            _cardRepository = unitOfWork.GetRepository<Card, Guid>();
            _boardRepository = unitOfWork.GetRepository<Board, Guid>();
            _boardMemberRepository = unitOfWork.GetRepository<BoardMember, Guid>();

            _permission = permission;
            _currentUser = currentUser;
            _realtime = realtime;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<ListDto>> Handle(CloneListCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<ListDto>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var sourceList = await _listRepository.GetByIdAsync(request.ListId);
            if (sourceList == null)
                return Result<ListDto>.Failure("List không tồn tại.");

            var board = await _boardRepository.GetByIdAsync(sourceList.BoardId)
                ?? throw new NotFoundException("Board không tồn tại.");

            await _permission.EnsureEditorAsync(board.Id, userId);

            var lists = await _listRepository.FindAsync(l => l.BoardId == board.Id);
            int newPosition = lists.Count == 0 ? 0 : lists.Max(l => l.Position) + 1;

            var newList = List.Create(board.Id, sourceList.Title, newPosition);
            await _listRepository.AddAsync(newList);

            if (!request.CopyCards)
            {
                await _unitOfWork.SaveChangesAsync();

                var dtoSimple = _mapper.Map<ListDto>(newList);
                await _realtime.SendToBoardAsync(board.Id, ListEvents.Created, dtoSimple);

                return Result<ListDto>.Success(dtoSimple);
            }

            var cards = await _cardRepository.FindAsync(c => c.ListId == sourceList.Id);

            var boardMembers = await _boardMemberRepository.FindAsync(m => m.BoardId == board.Id);
            var memberIds = boardMembers.Select(m => m.UserId).ToHashSet();

            foreach (var card in cards.OrderBy(c => c.Position))
            {
                var newCard = Card.Create(
                    listId: newList.Id,
                    title: card.Title,
                    position: card.Position,
                    background: card.Background,
                    description: card.Description
                );

                newCard.Label = card.Label;
                newCard.SetStatus(card.Status);
                newCard.SetStartDate(card.StartDate);
                newCard.SetDueDate(card.DueDate);

                if (card.ReminderEnabled && card.ReminderBeforeMinutes != null)
                    newCard.EnableReminder(card.ReminderBeforeMinutes.Value);

                await _cardRepository.AddAsync(newCard);

                var tasks = await _taskRepository.FindAsync(t => t.CardId == card.Id);

                foreach (var task in tasks)
                {
                    var newTask = Domain.Entities.Task.Create(newCard.Id, task.Title, task.Position);
                    await _taskRepository.AddAsync(newTask);

                    var subs = await _subTaskRepository.FindAsync(st => st.TaskId == task.Id);

                    foreach (var st in subs)
                    {
                        var newSt = SubTask.Create(newTask.Id, st.Title, st.Position);
                        newSt.UpdateStatus(st.Status);
                        newSt.SetDueDate(st.DueDate);

                        await _subTaskRepository.AddAsync(newSt);

                        var stAssignees = await _subTaskAssigneeRepository.FindAsync(a => a.SubTaskId == st.Id);
                        foreach (var sa in stAssignees)
                        {
                            if (memberIds.Contains(sa.UserId))
                            {
                                var newSa = SubTaskAssignee.Create(newSt.Id, sa.UserId);
                                await _subTaskAssigneeRepository.AddAsync(newSa);
                            }
                        }
                    }

                    var taskAssignees = await _cardAssigneeRepository.FindAsync(a => a.CardId == card.Id);
                    foreach (var ta in taskAssignees)
                    {
                        if (memberIds.Contains(ta.UserId))
                        {
                            var newTa = CardAssignee.Create(newCard.Id, ta.UserId);
                            await _cardAssigneeRepository.AddAsync(newTa);
                        }
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<ListDto>(newList);
            await _realtime.SendToBoardAsync(board.Id, ListEvents.Created, dto);

            return Result<ListDto>.Success(dto);
        }
    }
}
