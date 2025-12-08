using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Application.Common.Constants.EventNames;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.WorkSpaceMembers.Commands
{
    public class AddWorkSpaceMemberCommand : IRequest<Result>
    {
        public Guid WorkspaceId { get; set; }
        public Guid UserId { get; set; }
        public WorkSpaceRole Role { get; set; }
    }


    public class AddWorkSpaceMemberCommandValidator : AbstractValidator<AddWorkSpaceMemberCommand>
    {
        public AddWorkSpaceMemberCommandValidator()
        {
            RuleFor(x => x.WorkspaceId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.Role).IsInEnum();
        }
    }

    public class AddWorkSpaceMemberCommandHandler : IRequestHandler<AddWorkSpaceMemberCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<WorkspaceMember, Guid> _memberRepository;
        private readonly IPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtimeService;

        public AddWorkSpaceMemberCommandHandler(
            IUnitOfWork unitOfWork,
            IPermissionService permission,
            ICurrentUserService currentUser,
            IRealtimeService realtimeService)
        {
            _unitOfWork = unitOfWork;
            _memberRepository = unitOfWork.GetRepository<WorkspaceMember, Guid>();
            _permission = permission;
            _currentUser = currentUser;
            _realtimeService = realtimeService;
        }

        public async Task<Result> Handle(AddWorkSpaceMemberCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                throw new ForbiddenAccessException("Không xác định được người dùng.");

            var currentUserId = _currentUser.UserId.Value;

            await _permission.Workspace.EnsureAdminAsync(request.WorkspaceId, currentUserId);

            await _permission.Workspace.EnsureCanAssignRoleAsync(
                request.WorkspaceId,
                currentUserId,
                request.Role
            );

            var exists = await _memberRepository.AnyAsync(
                x => x.WorkSpaceId == request.WorkspaceId && x.UserId == request.UserId
            );

            if (exists)
                return Result.Failure("Thành viên đã tồn tại trong WorkSpace.");

            var member = WorkspaceMember.Create(
                request.WorkspaceId,
                request.UserId,
                request.Role
            );

            await _memberRepository.AddAsync(member);
            await _unitOfWork.SaveChangesAsync();

            await _realtimeService.SendToUserAsync(request.UserId, WorkspaceEvents.MemberAdded, member);
            await _realtimeService.SendToWorkspaceAsync(request.WorkspaceId, WorkspaceEvents.MemberAdded, member);

            return Result.Success("Thêm thành viên thành công.");
        }
    }

}
