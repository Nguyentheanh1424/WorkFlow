using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.WorkSpaceMembers.Commands
{
    public class UpdateWorkSpaceRoleCommand : IRequest<Result>
    {
        public Guid WorkspaceId { get; set; }
        public Guid UserId { get; set; }
        public WorkSpaceRole NewRole { get; set; }
    }


    public class UpdateRoleCommandValidator : AbstractValidator<UpdateWorkSpaceRoleCommand>
    {
        public UpdateRoleCommandValidator()
        {
            RuleFor(x => x.WorkspaceId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.NewRole).IsInEnum();
        }
    }

    public class UpdateRoleCommandHandler : IRequestHandler<UpdateWorkSpaceRoleCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<WorkspaceMember, Guid> _memberRepository;
        private readonly IPermissionService _permission;
        private readonly ICurrentUserService _currentUser;

        public UpdateRoleCommandHandler(
            IUnitOfWork unitOfWork,
            IPermissionService permission,
            ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _memberRepository = unitOfWork.GetRepository<WorkspaceMember, Guid>();
            _permission = permission;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(UpdateWorkSpaceRoleCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                throw new ForbiddenAccessException("Không xác định được người dùng.");

            var currentUserId = _currentUser.UserId.Value;

            await _permission.Workspace.EnsureAdminAsync(request.WorkspaceId, currentUserId);

            var member = await _memberRepository.FirstOrDefaultAsync(
                x => x.WorkSpaceId == request.WorkspaceId && x.UserId == request.UserId
            );

            if (member == null)
                throw new NotFoundException("Thành viên không tồn tại trong WorkSpace.");

            if (member.Role == WorkSpaceRole.Owner && request.NewRole != WorkSpaceRole.Owner)
            {
                if (await _permission.Workspace.IsLastOwnerAsync(request.WorkspaceId, request.UserId))
                    return Result.Failure("Không thể thay đổi vì đây là Owner duy nhất của Workspace.");
            }

            await _permission.Workspace.EnsureCanAssignRoleAsync(
                request.WorkspaceId,
                currentUserId,
                request.NewRole
            );

            await _permission.Workspace.EnsureCanModifyMemberRoleAsync(
                request.WorkspaceId,
                currentUserId,
                member.UserId
            );

            member.Role = request.NewRole;

            await _memberRepository.UpdateAsync(member);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Cập nhật quyền thành công.");
        }
    }
}
