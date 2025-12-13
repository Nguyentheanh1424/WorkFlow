using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.InviteLinks.Commands
{
    public record RevokeInviteLinkCommand(
        Guid InviteLinkId
    ) : IRequest<Result<bool>>;


    public class RevokeInviteLinkCommandValidator
        : AbstractValidator<RevokeInviteLinkCommand>
    {
        public RevokeInviteLinkCommandValidator()
        {
            RuleFor(x => x.InviteLinkId)
                .NotEmpty()
                .WithMessage("InviteLinkId không được để trống.");
        }
    }

    public class RevokeInviteLinkCommandHandler
        : IRequestHandler<RevokeInviteLinkCommand, Result<bool>>
    {
        private readonly IRepository<InviteLink, Guid> _inviteLinkRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IPermissionService _permission;
        private readonly IUnitOfWork _unitOfWork;

        public RevokeInviteLinkCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IPermissionService permission)
        {
            _inviteLinkRepository = unitOfWork.GetRepository<InviteLink, Guid>();
            _currentUser = currentUser;
            _permission = permission;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(
            RevokeInviteLinkCommand request,
            CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<bool>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var inviteLink = await _inviteLinkRepository.GetByIdAsync(request.InviteLinkId);
            if (inviteLink == null)
                return Result<bool>.Failure("Invite Link không tồn tại.");

            if (inviteLink.Type == InviteLinkType.WorkSpace)
            {
                await _permission.Workspace.EnsureAdminAsync(inviteLink.TargetId, userId);
            }
            else if (inviteLink.Type == InviteLinkType.Board)
            {
                await _permission.Board.EnsureOwnerAsync(inviteLink.TargetId, userId);
            }
            else
            {
                return Result<bool>.Failure("InviteLinkType không hợp lệ.");
            }

            inviteLink.Revoke();

            await _inviteLinkRepository.UpdateAsync(inviteLink);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}
