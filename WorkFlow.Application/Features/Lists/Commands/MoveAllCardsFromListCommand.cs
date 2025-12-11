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

namespace WorkFlow.Application.Features.Lists.Commands
{
    public record MoveAllCardsFromListCommand(Guid SourceListId, Guid TargetListId)
        : IRequest<Result>;

    public class MoveAllCardsFromListCommandValidator : AbstractValidator<MoveAllCardsFromListCommand>
    {
        public MoveAllCardsFromListCommandValidator()
        {
            RuleFor(x => x.SourceListId)
                .NotEmpty().WithMessage("SourceListId không được để trống.");

            RuleFor(x => x.TargetListId)
                .NotEmpty().WithMessage("TargetListId không được để trống.");

            RuleFor(x => x)
                .Must(x => x.SourceListId != x.TargetListId)
                .WithMessage("Nguồn và đích không được giống nhau.");
        }
    }

    public class MoveAllCardsFromListCommandHandler
        : IRequestHandler<MoveAllCardsFromListCommand, Result>
    {
        private readonly IRepository<List, Guid> _listRepository;
        private readonly IRepository<Card, Guid> _cardRepository;
        private readonly IRepository<Board, Guid> _boardRepository;

        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;
        private readonly IUnitOfWork _unitOfWork;

        public MoveAllCardsFromListCommandHandler(
            IBoardPermissionService permission,
            ICurrentUserService currentUser,
            IRealtimeService realtime,
            IUnitOfWork unitOfWork)
        {
            _listRepository = unitOfWork.GetRepository<List, Guid>();
            _boardRepository = unitOfWork.GetRepository<Board, Guid>();
            _cardRepository = unitOfWork.GetRepository<Card, Guid>();
            _permission = permission;
            _currentUser = currentUser;
            _realtime = realtime;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(MoveAllCardsFromListCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var sourceList = await _listRepository.GetByIdAsync(request.SourceListId)
                ?? throw new NotFoundException("List nguồn không tồn tại.");

            var targetList = await _listRepository.GetByIdAsync(request.TargetListId)
                ?? throw new NotFoundException("List đích không tồn tại.");

            await _permission.EnsureEditorAsync(sourceList.BoardId, userId);
            await _permission.EnsureEditorAsync(targetList.BoardId, userId);

            var sourceCards = await _cardRepository.FindAsync(c => c.ListId == sourceList.Id);
            var sourceOrdered = sourceCards.OrderBy(c => c.Position).ToList();

            if (sourceOrdered.Count == 0)
                return Result.Success();

            var targetCards = await _cardRepository.FindAsync(c => c.ListId == targetList.Id);
            int nextPosition = targetCards.Count == 0 ? 0 : targetCards.Max(c => c.Position) + 1;

            List<Guid> movedIds = new();

            foreach (var card in sourceOrdered)
            {
                movedIds.Add(card.Id);
                card.MoveToList(targetList.Id);
                card.MoveTo(nextPosition++);
                await _cardRepository.UpdateAsync(card);
            }

            await _unitOfWork.SaveChangesAsync();

            await _realtime.SendToBoardAsync(
                targetList.BoardId,
                BoardEvents.ListMoved,
                new
                {
                    SourceListId = sourceList.Id,
                    TargetListId = targetList.Id,
                    CardIds = movedIds
                });

            return Result.Success();
        }
    }
}
