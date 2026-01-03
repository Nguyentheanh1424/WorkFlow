using AutoMapper;
using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.Boards.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.Boards.Queries
{
    public class GetBoardsQuery : IRequest<Result<List<BoardDto>>>
    {
        public Guid WorkspaceId { get; set; }

        public string? Keyword { get; set; }
        public VisibilityBoard? Visibility { get; set; }
        public bool? Pinned { get; set; }
        public bool? IncludeArchived { get; set; }
        public BoardRole? Role { get; set; }
        public SortBoardsBy Sort { get; set; } = SortBoardsBy.CreatedAt;
    }

    public class GetBoardsQueryValidator : AbstractValidator<GetBoardsQuery>
    {
        public GetBoardsQueryValidator()
        {
            RuleFor(x => x.WorkspaceId)
                .NotEmpty().WithMessage("WorkspaceId không được để trống.");

            RuleFor(x => x.Sort)
                .IsInEnum()
                .WithMessage("Giá trị Sort không hợp lệ.");

            RuleFor(x => x.Visibility)
                .Must(v => v == null || Enum.IsDefined(typeof(VisibilityBoard), v))
                .WithMessage("Visibility không hợp lệ.");

            RuleFor(x => x.Role)
                .Must(r => r == null || Enum.IsDefined(typeof(BoardRole), r))
                .WithMessage("Role không hợp lệ.");
        }
    }

    public class GetBoardsQueryHandler
    : IRequestHandler<GetBoardsQuery, Result<List<BoardDto>>>
    {
        private readonly IRepository<Board, Guid> _boardRepository;
        private readonly IWorkSpacePermissionService _workspacePermission;
        private readonly IBoardPermissionService _boardPermissionService;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public GetBoardsQueryHandler(
            IUnitOfWork unitOfWork,
            IWorkSpacePermissionService workspacePermission,
            IBoardPermissionService boardPermissionService,
            ICurrentUserService currentUser,
            IMapper mapper)
        {
            _boardRepository = unitOfWork.GetRepository<Board, Guid>();
            _workspacePermission = workspacePermission;
            _boardPermissionService = boardPermissionService;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<Result<List<BoardDto>>> Handle(
            GetBoardsQuery request,
            CancellationToken cancellationToken)
        {
            if (_currentUser.UserId is null)
                return Result<List<BoardDto>>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            await _workspacePermission.EnsureMemberAsync(
                request.WorkspaceId,
                userId);

            IQueryable<Board> query = _boardRepository
                .GetAll()
                .Where(b => b.WorkSpaceId == request.WorkspaceId);

            if (request.IncludeArchived != true)
            {
                query = query.Where(b => !b.IsArchived);
            }

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim().ToLower();

                query = query.Where(b =>
                    b.Title.ToLower().Contains(keyword) ||
                    (b.Description != null &&
                     b.Description.ToLower().Contains(keyword)));
            }

            var visibleBoards = await ApplyVisibilityFilterAsync(query, userId);

            var filteredBoards = visibleBoards.AsQueryable();

            if (request.Visibility.HasValue)
            {
                query = query.Where(b =>
                    b.Visibility == request.Visibility.Value);
            }

            if (request.Pinned.HasValue)
            {
                query = query.Where(b =>
                    b.Pinned == request.Pinned.Value);
            }

            if (request.Role.HasValue)
            {
                query = await ApplyRoleFilterAsync(
                    query,
                    userId,
                    request.Role.Value);
            }

            query = request.Sort switch
            {
                SortBoardsBy.Title =>
                    query.OrderBy(b => b.Title),

                SortBoardsBy.UpdatedAt =>
                    query.OrderByDescending(b => b.UpdatedAt),

                _ =>
                    query.OrderByDescending(b => b.CreatedAt)
            };

            var boards = query.ToList();
            var result = _mapper.Map<List<BoardDto>>(boards);

            return Result<List<BoardDto>>.Success(result);
        }


        private async Task<List<Board>> ApplyVisibilityFilterAsync(
            IQueryable<Board> query,
            Guid userId)
        {
            var boards = query.ToList();

            var result = new List<Board>();

            foreach (var board in boards)
            {
                if (board.Visibility == VisibilityBoard.Public ||
                    board.Visibility == VisibilityBoard.Protected)
                {
                    result.Add(board);
                    continue;
                }

                if (board.Visibility == VisibilityBoard.Private)
                {
                    var role = await _boardPermissionService
                        .GetRoleAsync(board.Id, userId);

                    if (role != null)
                    {
                        result.Add(board);
                    }
                }
            }

            return result;
        }


        private async Task<IQueryable<Board>> ApplyRoleFilterAsync(
            IQueryable<Board> query,
            Guid userId,
            BoardRole role)
        {
            var boards = query.ToList();
            var result = new List<Board>();

            foreach (var board in boards)
            {
                var userRole = await _boardPermissionService
                    .GetRoleAsync(board.Id, userId);

                if (userRole == role)
                {
                    result.Add(board);
                }
            }

            return result.AsQueryable();
        }
    }
}
