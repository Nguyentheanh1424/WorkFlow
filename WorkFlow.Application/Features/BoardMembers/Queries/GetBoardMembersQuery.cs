using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.BoardMembers.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.BoardMembers.Queries
{
    public record GetBoardMembersQuery(Guid BoardId) : IRequest<Result<List<BoardMemberDto>>>;

    public class GetBoardMembersQueryHandler : IRequestHandler<GetBoardMembersQuery, Result<List<BoardMemberDto>>>
    {
        private readonly IRepository<BoardMember, Guid> _boardMemberRepository;
        private readonly IRepository<User, Guid> _userRepository;
        private readonly IPermissionService _permission;
        private readonly ICurrentUserService _currentUser;

        public GetBoardMembersQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IPermissionService permission,
            IMapper mapper)
        {
            _boardMemberRepository = unitOfWork.GetRepository<BoardMember, Guid>();
            _userRepository = unitOfWork.GetRepository<User, Guid>();
            _currentUser = currentUser;
            _permission = permission;
        }

        public async Task<Result<List<BoardMemberDto>>> Handle(GetBoardMembersQuery request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                throw new ForbiddenAccessException("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            await _permission.Board.EnsureViewerAsync(request.BoardId, userId);

            var members = await _boardMemberRepository
                .FindAsync(x => x.BoardId == request.BoardId);

            var userIds = members.Select(m => m.UserId).ToList();
            var users = await _userRepository.FindAsync(x => userIds.Contains(x.Id));

            var lookup = users.ToDictionary(x => x.Id);

            var result = members.Select(m =>
            {
                lookup.TryGetValue(m.UserId, out var user);

                return new BoardMemberDto
                {
                    UserId = m.UserId,
                    Name = user?.Name ?? string.Empty,
                    Role = m.Role,
                    JoinedAt = m.JoinedAt
                };
            }).ToList();

            return Result<List<BoardMemberDto>>.Success(result);
        }
    }
}
