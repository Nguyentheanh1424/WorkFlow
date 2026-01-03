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
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.Boards.Commands
{
    public record UpdateBoardVisibilityCommand(Guid BoardId, VisibilityBoard Visibility)
        : IRequest<Result>;

    public class UpdateBoardVisibilityCommandValidator
        : AbstractValidator<UpdateBoardVisibilityCommand>
    {
        public UpdateBoardVisibilityCommandValidator()
        {
            RuleFor(x => x.BoardId)
                .NotEmpty().WithMessage("BoardId không được để trống.");

            RuleFor(x => x.Visibility)
                .IsInEnum()
                .WithMessage("Visibility không hợp lệ.");
        }
    }

    public class UpdateBoardVisibilityCommandHandler
        : IRequestHandler<UpdateBoardVisibilityCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Board, Guid> _boardRepository;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;
        private readonly IMapper _mapper;

        public UpdateBoardVisibilityCommandHandler(
            IUnitOfWork unitOfWork,
            IBoardPermissionService permission,
            ICurrentUserService currentUser,
            IRealtimeService realtime,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _boardRepository = unitOfWork.GetRepository<Board, Guid>();
            _permission = permission;
            _currentUser = currentUser;
            _realtime = realtime;
            _mapper = mapper;
        }

        public async Task<Result> Handle(UpdateBoardVisibilityCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var board = await _boardRepository.GetByIdAsync(request.BoardId)
                ?? throw new NotFoundException("Board không tồn tại.");

            await _permission.EnsureOwnerAsync(board.Id, userId);

            board.ChangeVisibility(request.Visibility);

            await _boardRepository.UpdateAsync(board);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<BoardDto>(board);

            await _realtime.SendToBoardAsync(board.Id, "BoardNotification", new { Action = BoardEvents.Updated, Data = dto });
            await _realtime.SendToWorkspaceAsync(board.WorkSpaceId, "WorkspaceNotification", new { Action = BoardEvents.Updated, Data = dto });

            return Result.Success();
        }
    }
}
