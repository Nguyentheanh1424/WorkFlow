using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Constants.EventNames;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.BoardMembers.Commands
{
    public class UpdateBoardRoleCommand : IRequest<Result>
    {
        public Guid BoardId { get; set; }
        public Guid UserId { get; set; }
        public BoardRole NewRole { get; set; }
    }
    public class UpdateBoardRoleCommandValidator : AbstractValidator<UpdateBoardRoleCommand>
    {
        public UpdateBoardRoleCommandValidator()
        {
            RuleFor(x => x.BoardId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.NewRole).IsInEnum();
        }
    }
    public class UpdateBoardRoleCommandHandler
    : IRequestHandler<UpdateBoardRoleCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<BoardMember, Guid> _boardMemberRepository;
        private readonly IPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtime;

        public UpdateBoardRoleCommandHandler(
            IUnitOfWork unitOfWork,
            IPermissionService permission,
            ICurrentUserService currentUser,
            IRealtimeService realtime)
        {
            _unitOfWork = unitOfWork;
            _boardMemberRepository = unitOfWork.GetRepository<BoardMember, Guid>();
            _permission = permission;
            _currentUser = currentUser;
            _realtime = realtime;
        }

        public async Task<Result> Handle(UpdateBoardRoleCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                throw new ForbiddenAccessException("Không xác định được người dùng.");

            var currentUserId = _currentUser.UserId.Value;

            await _permission.Board.EnsureCanModifyMemberRoleAsync(
                request.BoardId,
                currentUserId,
                request.UserId
            );

            await _permission.Board.EnsureCanAssignRoleAsync(
                request.BoardId,
                currentUserId,
                request.NewRole
            );

            var member = await _boardMemberRepository.FirstOrDefaultAsync(
                x => x.BoardId == request.BoardId && x.UserId == request.UserId
            );

            if (member == null)
                throw new NotFoundException("Thành viên không tồn tại trong Board.");

            if (member.Role == BoardRole.Owner && request.NewRole != BoardRole.Owner)
            {
                if (await _permission.Board.IsLastOwnerAsync(request.BoardId, request.UserId))
                    return Result.Failure("Không thể thay đổi vì đây là Owner duy nhất của Board.");
            }

            member.Role = request.NewRole;

            await _boardMemberRepository.UpdateAsync(member);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _realtime.SendToUserAsync(
                request.UserId,
                BoardEvents.MemberUpdateRole,
                new { request.BoardId }
            );

            return Result.Success("Cập nhật quyền thành viên Board thành công.");
        }
    }

}
