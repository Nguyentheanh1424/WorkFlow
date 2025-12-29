using AutoMapper;
using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.BoardMembers.Dtos;
using WorkFlow.Application.Features.Boards.Dtos;
using WorkFlow.Application.Features.Cards.Dtos;
using WorkFlow.Application.Features.Lists.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.Boards.Queries
{
    public record GetBoardFullDetailQuery(Guid BoardId)
        : IRequest<Result<BoardFullDetailDto>>;

    public class GetBoardFullDetailQueryValidator : AbstractValidator<GetBoardFullDetailQuery>
    {
        public GetBoardFullDetailQueryValidator()
        {
            RuleFor(x => x.BoardId)
                .NotEmpty()
                .WithMessage("BoardId không được để trống.");
        }
    }

    public class GetBoardFullDetailQueryHandler
        : IRequestHandler<GetBoardFullDetailQuery, Result<BoardFullDetailDto>>
    {
        private readonly IRepository<Board, Guid> _boardRepository;
        private readonly IRepository<List, Guid> _listRepository;
        private readonly IRepository<Card, Guid> _cardRepository;
        private readonly IRepository<BoardMember, Guid> _boardMemberRepository;
        private readonly IRepository<User, Guid> _userRepository;

        private readonly IBoardPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public GetBoardFullDetailQueryHandler(
            IUnitOfWork unitOfWork,
            IBoardPermissionService permission,
            ICurrentUserService currentUser,
            IMapper mapper)
        {
            _boardRepository = unitOfWork.GetRepository<Board, Guid>();
            _listRepository = unitOfWork.GetRepository<List, Guid>();
            _cardRepository = unitOfWork.GetRepository<Card, Guid>();
            _boardMemberRepository = unitOfWork.GetRepository<BoardMember, Guid>();
            _userRepository = unitOfWork.GetRepository<User, Guid>();

            _permission = permission;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<Result<BoardFullDetailDto>> Handle(GetBoardFullDetailQuery request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<BoardFullDetailDto>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var board = await _boardRepository.GetByIdAsync(request.BoardId)
                ?? throw new NotFoundException("Board không tồn tại.");

            await _permission.EnsureViewerAsync(board.Id, userId);

            var lists = (await _listRepository
                .FindAsync(l => l.BoardId == board.Id && !l.IsArchived))
                .OrderBy(l => l.Position)
                .ToList();

            var listIds = lists.Select(l => l.Id).ToList();

            List<Card> cards = new();

            if (listIds.Count > 0)
            {
                cards = (await _cardRepository
                        .FindAsync(c => listIds.Contains(c.ListId)))
                        .OrderBy(c => c.ListId)
                        .ThenBy(c => c.Position)
                        .ToList();
            }

            var members = await _boardMemberRepository.FindAsync(m => m.BoardId == board.Id);

            var memberUserIds = members.Select(m => m.UserId).ToList();
            var users = await _userRepository.FindAsync(u => memberUserIds.Contains(u.Id));
            var dictUser = users.ToDictionary(u => u.Id);

            var memberDtos = members.Select(m =>
            {
                dictUser.TryGetValue(m.UserId, out var user);

                return new BoardMemberDto
                {
                    UserId = m.UserId,
                    Name = user?.Name ?? string.Empty,
                    Role = m.Role,
                    JoinedAt = m.JoinedAt
                };
            }).ToList();

            var currentRole = await _permission.GetRoleAsync(board.Id, userId)
                ?? BoardRole.Viewer;

            var dto = new BoardFullDetailDto
            {
                Board = _mapper.Map<BoardDto>(board),
                Lists = _mapper.Map<List<ListDto>>(lists),
                Cards = _mapper.Map<List<CardDto>>(cards),
                Members = memberDtos,
                CurrentUserRole = currentRole
            };

            return Result<BoardFullDetailDto>.Success(dto);
        }
    }
}
