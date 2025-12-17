using MediatR;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.WorkSpaceMembers.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.WorkSpaceMembers.Queries
{
    public record GetWorkSpaceMembersQuery(Guid WorkspaceId)
        : IRequest<Result<List<WorkSpaceMemberDto>>>;

    public class GetWorkSpaceMembersQueryHandler
        : IRequestHandler<GetWorkSpaceMembersQuery, Result<List<WorkSpaceMemberDto>>>
    {
        private readonly IRepository<WorkspaceMember, Guid> _memberRepository;
        private readonly IRepository<User, Guid> _userRepository;
        private readonly IPermissionService _permission;
        private readonly ICurrentUserService _currentUser;

        public GetWorkSpaceMembersQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IPermissionService permission)
        {
            _memberRepository = unitOfWork.GetRepository<WorkspaceMember, Guid>();
            _userRepository = unitOfWork.GetRepository<User, Guid>();
            _currentUser = currentUser;
            _permission = permission;
        }

        public async Task<Result<List<WorkSpaceMemberDto>>> Handle(GetWorkSpaceMembersQuery request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                throw new ForbiddenAccessException("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            await _permission.Workspace.EnsureMemberAsync(request.WorkspaceId, userId);

            var members = await _memberRepository.FindAsync(
                x => x.WorkSpaceId == request.WorkspaceId
            );

            var userIds = members.Select(m => m.UserId).ToList();
            var users = await _userRepository.FindAsync(x => userIds.Contains(x.Id));

            var lookup = users.ToDictionary(x => x.Id, x => x);

            var dtos = members.Select(m =>
            {
                lookup.TryGetValue(m.UserId, out var user);

                return new WorkSpaceMemberDto
                {
                    UserId = m.UserId,
                    Name = user?.Name ?? string.Empty,
                    Email = user?.Email ?? string.Empty,
                    Role = m.Role,
                    JoinedAt = m.JoinedAt
                };
            }).ToList();

            return Result<List<WorkSpaceMemberDto>>.Success(dtos);
        }
    }
}
