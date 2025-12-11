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
    public record UpdateBoardTitleCommand(Guid BoardId, string Title) : IRequest<Result<BoardDto>>;

    public class UpdateBoardTitleCommandValidator : AbstractValidator<UpdateBoardTitleCommand>
    {
        public UpdateBoardTitleCommandValidator()
        {
            RuleFor(x => x.BoardId)
                .NotEmpty().WithMessage("BoardId không được để trống.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Tên board không được để trống.")
                .MaximumLength(255).WithMessage("Tên board không được vượt quá 255 ký tự.");
        }
    }

    public class UpdateBoardTitleCommandHandler : IRequestHandler<UpdateBoardTitleCommand, Result<BoardDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Board, Guid> _boardRepository;
        private readonly IRealtimeService _realtimeService;
        private readonly ICurrentUserService _currentUser;
        private readonly IPermissionService _permission;
        private readonly IMapper _mapper;

        public UpdateBoardTitleCommandHandler(
            IUnitOfWork unitOfWork,
            IRealtimeService realtimeService,
            ICurrentUserService currentUser,
            IPermissionService permission,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _realtimeService = realtimeService;
            _currentUser = currentUser;
            _permission = permission;
            _mapper = mapper;

            _boardRepository = unitOfWork.GetRepository<Board, Guid>();
        }

        public async Task<Result<BoardDto>> Handle(UpdateBoardTitleCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<BoardDto>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var board = await _boardRepository.GetByIdAsync(request.BoardId)
                ?? throw new NotFoundException("Board không tồn tại.");

            await _permission.Board.EnsureEditorAsync(board.Id, userId);

            board.Rename(request.Title);

            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<BoardDto>(board);

            await _realtimeService.SendToBoardAsync(board.Id, BoardEvents.Updated, dto);
            await _realtimeService.SendToWorkspaceAsync(board.WorkSpaceId, BoardEvents.Updated, dto);

            return Result<BoardDto>.Success(dto);
        }
    }
}
