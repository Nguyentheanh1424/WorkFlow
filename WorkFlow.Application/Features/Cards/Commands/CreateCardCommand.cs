using AutoMapper;
using FluentValidation;
using MediatR;
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
    public record CreateCardCommand(Guid ListId, string Title)
        : IRequest<Result<CardDto>>;

    public class CreateCardCommandValidator : AbstractValidator<CreateCardCommand>
    {
        public CreateCardCommandValidator()
        {
            RuleFor(x => x.ListId)
                .NotEmpty().WithMessage("ListId không được để trống.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title không được để trống.")
                .MaximumLength(200);
        }
    }

    public class CreateCardCommandHandler
        : IRequestHandler<CreateCardCommand, Result<CardDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;
        private readonly IMapper _mapper;

        public CreateCardCommandHandler(
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

        public async Task<Result<CardDto>> Handle(CreateCardCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<CardDto>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var listRepo = _unitOfWork.GetRepository<List, Guid>();
            var boardRepo = _unitOfWork.GetRepository<Board, Guid>();
            var cardRepo = _unitOfWork.GetRepository<Card, Guid>();

            var list = await listRepo.GetByIdAsync(request.ListId);
            if (list == null)
                return Result<CardDto>.Failure("List không tồn tại.");

            var board = await boardRepo.GetByIdAsync(list.BoardId)
                ?? throw new NotFoundException("Board không tồn tại.");

            await _permission.EnsureEditorAsync(board.Id, userId);

            var cards = await cardRepo.FindAsync(c => c.ListId == list.Id);
            int nextPosition = cards.Count == 0 ? 0 : cards.Max(c => c.Position) + 1;

            var card = Card.Create(
                listId: list.Id,
                title: request.Title,
                position: nextPosition
            );

            await cardRepo.AddAsync(card);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<CardDto>(card);

            await _realtime.SendToBoardAsync(board.Id, CardEvents.Created, dto);

            return Result<CardDto>.Success(dto);
        }
    }
}
