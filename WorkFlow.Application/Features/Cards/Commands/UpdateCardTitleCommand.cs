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
    public record UpdateCardTitleCommand(Guid CardId, string Title)
        : IRequest<Result<CardDto>>;

    public class UpdateCardTitleCommandValidator : AbstractValidator<UpdateCardTitleCommand>
    {
        public UpdateCardTitleCommandValidator()
        {
            RuleFor(x => x.CardId).NotEmpty();

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title không được để trống.")
                .MaximumLength(200);
        }
    }

    public class UpdateCardTitleCommandHandler
        : IRequestHandler<UpdateCardTitleCommand, Result<CardDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;
        private readonly IMapper _mapper;

        public UpdateCardTitleCommandHandler(
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

        public async Task<Result<CardDto>> Handle(UpdateCardTitleCommand request, CancellationToken cancellationToken)
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

            var list = await listRepo.GetByIdAsync(card.ListId)
                ?? throw new NotFoundException("List không tồn tại.");

            var board = await boardRepo.GetByIdAsync(list.BoardId)
                ?? throw new NotFoundException("Board không tồn tại.");

            await _permission.EnsureEditorAsync(board.Id, userId);

            card.Rename(request.Title);

            await cardRepo.UpdateAsync(card);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<CardDto>(card);

            await _realtime.SendToBoardAsync(board.Id, CardEvents.Updated, dto);

            return Result<CardDto>.Success(dto);
        }
    }
}
