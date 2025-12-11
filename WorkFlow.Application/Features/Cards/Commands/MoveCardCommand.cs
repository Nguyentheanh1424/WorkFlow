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
using WorkFlow.Application.Features.Cards.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Cards.Commands
{
    public record MoveCardCommand(
        Guid CardId,
        Guid ToListId,
        int NewPosition
    ) : IRequest<Result<CardDto>>;

    public class MoveCardCommandValidator : AbstractValidator<MoveCardCommand>
    {
        public MoveCardCommandValidator()
        {
            RuleFor(x => x.CardId).NotEmpty();
            RuleFor(x => x.ToListId).NotEmpty();
            RuleFor(x => x.NewPosition).GreaterThanOrEqualTo(0);
        }
    }

    public class MoveCardCommandHandler
        : IRequestHandler<MoveCardCommand, Result<CardDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;
        private readonly IMapper _mapper;

        public MoveCardCommandHandler(
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

        public async Task<Result<CardDto>> Handle(MoveCardCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<CardDto>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var cardRepo = _unitOfWork.GetRepository<Card, Guid>();
            var listRepo = _unitOfWork.GetRepository<List, Guid>();
            var boardRepo = _unitOfWork.GetRepository<Board, Guid>();

            var card = await cardRepo.GetByIdAsync(request.CardId);
            if (card == null)
                return Result<CardDto>.Failure("Card không tồn tại.");

            var fromListId = card.ListId;

            var fromList = await listRepo.GetByIdAsync(fromListId)
                ?? throw new NotFoundException("List nguồn không tồn tại.");

            var board = await boardRepo.GetByIdAsync(fromList.BoardId)
                ?? throw new NotFoundException("Board không tồn tại.");

            await _permission.EnsureEditorAsync(board.Id, userId);

            var toList = await listRepo.GetByIdAsync(request.ToListId)
                ?? throw new NotFoundException("List đích không tồn tại.");

            if (toList.BoardId != board.Id)
                return Result<CardDto>.Failure("Không hỗ trợ move card sang board khác.");

            var fromCards = await cardRepo.FindAsync(c => c.ListId == fromListId);
            var toCards = request.ToListId == fromListId
                ? fromCards
                : await cardRepo.FindAsync(c => c.ListId == request.ToListId);

            var movedCard = fromCards.First(c => c.Id == card.Id);
            fromCards.Remove(movedCard);

            int pos = 0;
            foreach (var c in fromCards.OrderBy(c => c.Position))
            {
                c.MoveTo(pos++);
                await cardRepo.UpdateAsync(c);
            }

            movedCard.MoveToList(request.ToListId);

            var targetListCards = toCards.Where(c => c.Id != movedCard.Id).ToList();

            targetListCards.Insert(request.NewPosition, movedCard);

            pos = 0;
            foreach (var c in targetListCards)
            {
                c.MoveTo(pos++);
                await cardRepo.UpdateAsync(c);
            }

            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<CardDto>(movedCard);

            await _realtime.SendToBoardAsync(board.Id, CardEvents.Moved, new
            {
                CardId = movedCard.Id,
                FromListId = fromListId,
                ToListId = request.ToListId,
                NewPosition = request.NewPosition
            });

            return Result<CardDto>.Success(dto);
        }
    }
}
