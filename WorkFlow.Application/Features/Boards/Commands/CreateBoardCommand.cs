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
    public record CreateBoardCommand(CreateBoardDto Board) : IRequest<Result<BoardDto>>;

    public class CreateBoardCommandValidator : AbstractValidator<CreateBoardCommand>
    {
        public CreateBoardCommandValidator()
        {
            RuleFor(x => x.Board.WorkspaceId)
                .NotEmpty().WithMessage("WorkspaceId không được để trống.");

            RuleFor(x => x.Board.Title)
                .NotEmpty().WithMessage("Tên Board không được để trống.")
                .MaximumLength(255).WithMessage("Tên Board không được vượt quá 255 ký tự.");

            RuleFor(x => x.Board.Visibility)
                .IsInEnum()
                .WithMessage("Visibility không hợp lệ.");
        }
    }

    public class CreateBoardCommandHandler : IRequestHandler<CreateBoardCommand, Result<BoardDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Board, Guid> _boardRepository;
        private readonly IRepository<WorkSpace, Guid> _workspaceRepository;
        private readonly IRepository<BoardMember, Guid> _boardMemberRepository;
        private readonly IRealtimeService _realtimeService;
        private readonly ICurrentUserService _currentUser;
        private readonly IPermissionService _permission;
        private readonly IMapper _mapper;

        public CreateBoardCommandHandler(
            IUnitOfWork unitOfWork,
            IRealtimeService realtimeService,
            ICurrentUserService currentUser,
            IPermissionService permission,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _realtimeService = realtimeService;
            _currentUser = currentUser;
            _mapper = mapper;
            _permission = permission;

            _boardRepository = unitOfWork.GetRepository<Board, Guid>();
            _workspaceRepository = unitOfWork.GetRepository<WorkSpace, Guid>();
            _boardMemberRepository = unitOfWork.GetRepository<BoardMember, Guid>();
        }

        public async Task<Result<BoardDto>> Handle(CreateBoardCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<BoardDto>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var workSpace = await _workspaceRepository.GetByIdAsync(request.Board.WorkspaceId)
                ?? throw new NotFoundException("Workspace không tồn tại.");

            await _permission.Workspace.EnsureMemberAsync(workSpace.Id, userId);

            var board = Board.Create(
                request.Board.WorkspaceId,
                userId,
                request.Board.Title,
                request.Board.Visibility,
                request.Board.Description,
                request.Board.Background
            );

            await _boardRepository.AddAsync(board);

            var ownerMember = BoardMember.Create(board.Id, userId, BoardRole.Owner);
            await _boardMemberRepository.AddAsync(ownerMember);

            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<BoardDto>(board);

            await _realtimeService.SendToBoardAsync(board.Id, BoardEvents.Updated, dto);
            await _realtimeService.SendToWorkspaceAsync(board.WorkSpaceId, BoardEvents.Updated, dto);

            return Result<BoardDto>.Success(dto);
        }
    }
}
