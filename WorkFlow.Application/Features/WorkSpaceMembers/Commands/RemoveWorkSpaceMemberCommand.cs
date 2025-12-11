using AutoMapper.Execution;
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
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.WorkSpaceMembers.Commands
{
    public class RemoveWorkSpaceMemberCommand : IRequest<Result>
    {
        public Guid WorkspaceId { get; set; }
        public Guid TargetUserId { get; set; }
    }

    public class RemoveWorkSpaceMemberCommandValidator : AbstractValidator<RemoveWorkSpaceMemberCommand>
    {
        public RemoveWorkSpaceMemberCommandValidator()
        {
            RuleFor(x => x.WorkspaceId).NotEmpty();
            RuleFor(x => x.TargetUserId).NotEmpty();
        }
    }

    public class RemoveWorkSpaceMemberCommandHandler
    : IRequestHandler<RemoveWorkSpaceMemberCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<WorkspaceMember, Guid> _memberRepository;
        private readonly IPermissionService _permission;
        private readonly ICurrentUserService _currentUser;
        private readonly IRealtimeService _realtimeService;

        public RemoveWorkSpaceMemberCommandHandler(
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

        public async Task<Result> Handle(RemoveWorkSpaceMemberCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                throw new ForbiddenAccessException("Không xác định được người dùng.");

            var currentUserId = _currentUser.UserId.Value;

            var targetMember = await _memberRepository.FirstOrDefaultAsync(
                x => x.WorkSpaceId == request.WorkspaceId && x.UserId == request.TargetUserId
            );

            if (targetMember == null)
                throw new NotFoundException("Thành viên không tồn tại trong WorkSpace.");

            bool isSelfRemoving = request.TargetUserId == currentUserId;

            if (!isSelfRemoving)
            {
                await _permission.Workspace.EnsureAdminAsync(request.WorkspaceId, currentUserId);

                if (targetMember.Role == WorkSpaceRole.Owner)
                {
                    var currentRole = await _permission.Workspace.GetRoleAsync(request.WorkspaceId, currentUserId);

                    if (currentRole != WorkSpaceRole.Owner)
                        return Result.Failure("Bạn không thể xoá Owner khi bạn không phải Owner.");
                }
            }

            bool isLastOwner = await _permission.Workspace.IsLastOwnerAsync(request.WorkspaceId, request.TargetUserId);

            if (isLastOwner)
            {
                if (isSelfRemoving)
                    return Result.Failure("Bạn là Owner duy nhất, không thể tự xoá chính mình.");

                return Result.Failure("Không thể xoá Owner cuối cùng của Workspace.");
            }

            await _memberRepository.DeleteAsync(targetMember);
            await _unitOfWork.SaveChangesAsync();

            var payload = new
            {
                WorkSpaceId = request.WorkspaceId,
                UserId = request.TargetUserId,
                Status = true,
            };

            await _realtimeService.SendToUserAsync(request.TargetUserId, WorkspaceEvents.MemberRemoved, payload);
            await _realtimeService.SendToWorkspaceAsync(request.WorkspaceId, WorkspaceEvents.MemberRemoved, payload);

            return Result.Success("Xoá thành viên thành công.");
        }
    }

}
