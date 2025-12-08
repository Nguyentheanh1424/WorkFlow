using AutoMapper;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repository;
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
        private readonly IRepository<BoardMember, Guid> _boardMemberRepository;
        private readonly IWorkspacePermissionService _workspacePermission;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public GetBoardsQueryHandler(
            IUnitOfWork unitOfWork,
            IWorkspacePermissionService workspacePermission,
            ICurrentUserService currentUser,
            IMapper mapper)
        {
            _boardRepository = unitOfWork.GetRepository<Board, Guid>();
            _boardMemberRepository = unitOfWork.GetRepository<BoardMember, Guid>();
            _workspacePermission = workspacePermission;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<Result<List<BoardDto>>> Handle(GetBoardsQuery request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<List<BoardDto>>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            await _workspacePermission.EnsureMemberAsync(request.WorkspaceId, userId);

            var query = _boardRepository
                .GetAll()
                .Where(b => b.WorkspaceId == request.WorkspaceId);

            if (request.IncludeArchived != true)
            {
                query = query.Where(b => !b.IsArchived);
            }

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim().ToLower();

                query = query.Where(b =>
                    b.Title.ToLower().Contains(keyword) ||
                    (b.Description != null && b.Description.ToLower().Contains(keyword)));
            }

            if (request.Visibility.HasValue)
            {
                var vis = request.Visibility.Value;
                query = query.Where(b => b.Visibility == vis);
            }

            if (request.Pinned.HasValue)
            {
                query = query.Where(b => b.Pinned == request.Pinned.Value);
            }

            if (request.Role.HasValue)
            {
                var role = request.Role.Value;

                var memberList = await _boardMemberRepository.FindAsync(
                    m => m.UserId == userId && m.Role == role
                );

                var allowedIds = memberList.Select(m => m.BoardId).ToHashSet();

                query = query.Where(b => allowedIds.Contains(b.Id));
            }

            query = request.Sort switch
            {
                SortBoardsBy.Title => query.OrderBy(b => b.Title),
                SortBoardsBy.UpdatedAt => query.OrderByDescending(b => b.UpdatedAt),
                _ => query.OrderByDescending(b => b.CreatedAt)
            };

            var boards = query.ToList();
            var resultDto = _mapper.Map<List<BoardDto>>(boards);

            return Result<List<BoardDto>>.Success(resultDto);
        }
    }
}
