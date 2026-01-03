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
    public record UpdateBoardLabelsCommand(Guid BoardId, int[]? Labels)
        : IRequest<Result<BoardDto>>;

    public class UpdateBoardLabelsCommandValidator : AbstractValidator<UpdateBoardLabelsCommand>
    {
        public UpdateBoardLabelsCommandValidator()
        {
            RuleFor(x => x.BoardId).NotEmpty();

            RuleFor(x => x.Labels)
                .Must(l => l == null || l.Length <= 20)
                .WithMessage("Labels quá dài.");

            RuleForEach(x => x.Labels)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(20)
                .WithMessage("Mã màu không hợp lệ.");
        }
    }

    public class UpdateBoardLabelsCommandHandler : IRequestHandler<UpdateBoardLabelsCommand, Result<BoardDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Board, Guid> _boardRepository;
        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtimeService;
        private readonly IMapper _mapper;

        public UpdateBoardLabelsCommandHandler(
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
            _realtimeService = realtime;
            _mapper = mapper;
        }

        public async Task<Result<BoardDto>> Handle(UpdateBoardLabelsCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<BoardDto>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var board = await _boardRepository.GetByIdAsync(request.BoardId)
                ?? throw new NotFoundException("Board không tồn tại.");

            await _permission.EnsureEditorAsync(board.Id, userId);

            board.UpdateLabels(request.Labels);

            await _boardRepository.UpdateAsync(board);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<BoardDto>(board);

            await _realtimeService.SendToBoardAsync(board.Id, "BoardNotification", new { Action = BoardEvents.Updated, Data = dto });
            await _realtimeService.SendToWorkspaceAsync(board.WorkSpaceId, "WorkspaceNotification", new { Action = BoardEvents.Updated, Data = dto });

            return Result<BoardDto>.Success(dto);
        }
    }
}
