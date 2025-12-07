using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using WorkFlow.Application.Common.Constants.EventNames;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.BoardMembers.Commands
{
    public record RemoveBoardMemberCommand(Guid BoardId, Guid UserId)
        : IRequest<Result<bool>>;

    public class RemoveBoardMemberCommandHandler
        : IRequestHandler<RemoveBoardMemberCommand, Result<bool>>
    {
        private readonly IRepository<BoardMember, Guid> _boardMemberRepository;
        private readonly IRepository<User, Guid> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;
        private readonly IPermissionService _permission;

        public RemoveBoardMemberCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IRealtimeService realtime,
            IPermissionService permission)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _realtime = realtime;
            _permission = permission;

            _boardMemberRepository = unitOfWork.GetRepository<BoardMember, Guid>();
            _userRepository = unitOfWork.GetRepository<User, Guid>();
        }

        public async Task<Result<bool>> Handle(RemoveBoardMemberCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                throw new ForbiddenAccessException("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var member = await _boardMemberRepository.FirstOrDefaultAsync(
                x => x.BoardId == request.BoardId && x.UserId == request.UserId
            ) ?? throw new NotFoundException("User không thuộc board.");

            bool isSelfRemoving = request.UserId == userId;

            if (!isSelfRemoving)
            {
                await _permission.Board.EnsureOwnerAsync(request.BoardId, userId);
            }

            if (member.Role == BoardRole.Owner)
            {
                bool isLastOwner = await _permission.Board.IsLastOwnerAsync(request.BoardId, member.UserId);

                if (isLastOwner)
                    return Result<bool>.Failure("Không thể xoá Owner cuối cùng.");
            }

            await _boardMemberRepository.DeleteAsync(member);
            await _unitOfWork.SaveChangesAsync();

            await _realtime.SendToUserAsync(
                request.UserId,
                BoardEvents.MemberRemoved,
                new { request.BoardId }
            );

            return Result<bool>.Success(true);
        }
    }
}
