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
    public record UpdateCardDatesCommand(
        Guid CardId,
        DateTime? StartDate,
        DateTime? DueDate,
        bool ReminderEnabled,
        int? ReminderBeforeMinutes
        ) : IRequest<Result<CardDto>>;

    public class UpdateCardDatesCommandValidator : AbstractValidator<UpdateCardDatesCommand>
    {
        public UpdateCardDatesCommandValidator()
        {
            RuleFor(x => x.CardId).NotEmpty();

            RuleFor(x => x)
                .Must(x =>
                {
                    if (x.StartDate == null || x.DueDate == null) return true;
                    return x.DueDate >= x.StartDate;
                })
                .WithMessage("DueDate không được nhỏ hơn StartDate.");



            RuleFor(x => x.ReminderBeforeMinutes)
                .NotNull()
                .When(x => x.ReminderEnabled)
                .WithMessage("ReminderBeforeMinutes phải có giá trị khi ReminderEnabled = true.");

            RuleFor(x => x.ReminderBeforeMinutes)
                .Must(x => x == null)
                .When(x => !x.ReminderEnabled)
                .WithMessage("Không được gửi ReminderBeforeMinutes khi ReminderEnabled = false.");

            RuleFor(x => x.ReminderBeforeMinutes)
                .Must((cmd, val) =>
                {
                    if (!cmd.ReminderEnabled) return true;
                    return val is > 0 and <= 10080; // tối đa 7 ngày trước
                })
                .WithMessage("ReminderBeforeMinutes không hợp lệ.");
        }
    }

    public class UpdateCardDatesCommandHandler
        : IRequestHandler<UpdateCardDatesCommand, Result<CardDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;
        private readonly IMapper _mapper;

        public UpdateCardDatesCommandHandler(
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

        public async Task<Result<CardDto>> Handle(UpdateCardDatesCommand request, CancellationToken cancellationToken)
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

            card.SetStartDate(request.StartDate);
            card.SetDueDate(request.DueDate);

            if (request.ReminderEnabled)
                card.EnableReminder(request.ReminderBeforeMinutes!.Value);
            else
                card.DisableReminder();

            await cardRepo.UpdateAsync(card);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<CardDto>(card);

            await _realtime.SendToBoardAsync(board.Id, CardEvents.Updated, dto);

            return Result<CardDto>.Success(dto);
        }
    }
}
