using AutoMapper;
using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Constants.EventNames;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.Boards.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Boards.Commands
{
    public record DuplicateBoardCommand(Guid BoardId, bool CopyLists = true, bool CopyCards = true
        ) : IRequest<Result<BoardDto>>;

    public class DuplicateBoardCommandValidator : AbstractValidator<DuplicateBoardCommand>
    {
        public DuplicateBoardCommandValidator()
        {
            RuleFor(x => x.BoardId)
                .NotEmpty().WithMessage("BoardId không được để trống.");

            RuleFor(x => x)
                .Must(x => !(x.CopyCards && !x.CopyLists))
                .WithMessage("Không thể copy Cards nếu không copy Lists.");
        }
    }

    public class DuplicateBoardCommandHandler
        : IRequestHandler<DuplicateBoardCommand, Result<BoardDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Board, Guid> _boardRepository;
        private readonly IRepository<List, Guid> _listRepository;
        private readonly IRepository<Card, Guid> _cardRepository;
        private readonly IRepository<BoardMember, Guid> _boardMemberRepository;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;
        private readonly IMapper _mapper;

        public DuplicateBoardCommandHandler(
            IUnitOfWork unitOfWork,
            IBoardPermissionService permission,
            ICurrentUserService currentUser,
            IRealtimeService realtime,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _boardRepository = unitOfWork.GetRepository<Board, Guid>();
            _listRepository = unitOfWork.GetRepository<List, Guid>();
            _cardRepository = unitOfWork.GetRepository<Card, Guid>();
            _boardMemberRepository = unitOfWork.GetRepository<BoardMember, Guid>();

            _permission = permission;
            _currentUser = currentUser;
            _realtime = realtime;
            _mapper = mapper;
        }

        public async Task<Result<BoardDto>> Handle(DuplicateBoardCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<BoardDto>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var sourceBoard = await _boardRepository.GetByIdAsync(request.BoardId)
                ?? throw new NotFoundException("Board không tồn tại.");

            await _permission.EnsureViewerAsync(sourceBoard.Id, userId);

            var newBoard = Board.Create(
                workspaceId: sourceBoard.WorkSpaceId,
                ownerId: userId,
                title: $"{sourceBoard.Title} (Copy)",
                visibility: sourceBoard.Visibility,
                background: sourceBoard.Background,
                description: sourceBoard.Description,
                label: sourceBoard.Label
            );

            await _boardRepository.AddAsync(newBoard);

            var owner = BoardMember.Create(newBoard.Id, userId, Domain.Enums.BoardRole.Owner);
            await _boardMemberRepository.AddAsync(owner);

            if (!request.CopyLists)
            {
                await _unitOfWork.SaveChangesAsync();

                var boardDto = _mapper.Map<BoardDto>(newBoard);
                await _realtime.SendToWorkspaceAsync(newBoard.WorkSpaceId, BoardEvents.Created, boardDto);

                return Result<BoardDto>.Success(boardDto);
            }

            var sourceLists = await _listRepository
                .FindAsync(l => l.BoardId == sourceBoard.Id);

            var listMap = new Dictionary<Guid, Guid>();

            foreach (var list in sourceLists.OrderBy(l => l.Position))
            {
                var newList = List.Create(
                    boardId: newBoard.Id,
                    title: list.Title,
                    position: list.Position,
                    isArchived: list.IsArchived
                );

                await _listRepository.AddAsync(newList);
                listMap[list.Id] = newList.Id;
            }

            await _unitOfWork.SaveChangesAsync();

            var sourceCards = await _cardRepository
                .FindAsync(c => listMap.Keys.Contains(c.ListId));

            foreach (var card in sourceCards.OrderBy(c => c.Position))
            {
                var newCard = Card.Create(
                    listId: listMap[card.ListId],
                    title: card.Title,
                    position: card.Position,
                    background: card.Background,
                    description: card.Description
                );

                newCard.Label = card.Label;
                newCard.SetStatus(card.Status);
                newCard.SetStartDate(card.StartDate);
                newCard.SetDueDate(card.DueDate);

                if (card.ReminderEnabled && card.ReminderBeforeMinutes.HasValue)
                    newCard.EnableReminder(card.ReminderBeforeMinutes.Value);

                await _cardRepository.AddAsync(newCard);
            }

            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<BoardDto>(newBoard);

            await _realtime.SendToUserAsync(userId, WorkspaceEvents.BoardAdded, dto);
            await _realtime.SendToWorkspaceAsync(newBoard.WorkSpaceId, WorkspaceEvents.BoardAdded, dto);

            return Result<BoardDto>.Success(dto);
        }
    }
}
