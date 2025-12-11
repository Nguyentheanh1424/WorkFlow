using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Constants.EventNames;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.WorkSpaces.Commands
{
    public record UpdateWorkspaceTypeCommand(Guid WorkspaceId, WorkSpaceType Type)
    : IRequest<Result>;


    public class UpdateWorkspaceTypeCommandValidator : AbstractValidator<UpdateWorkspaceTypeCommand>
    {
        public UpdateWorkspaceTypeCommandValidator()
        {
            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Type không hợp lệ.");
        }
    }

    public class UpdateWorkspaceTypeCommandHandler
    : IRequestHandler<UpdateWorkspaceTypeCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IPermissionService _permission;
        private readonly IRealtimeService _realtime;

        public UpdateWorkspaceTypeCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IPermissionService permission,
            IRealtimeService realtime)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _permission = permission;
            _realtime = realtime;
        }

        public async Task<Result> Handle(UpdateWorkspaceTypeCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result.Failure("Không xác định người dùng.");

            var userId = _currentUser.UserId.Value;

            var repo = _unitOfWork.GetRepository<WorkSpace, Guid>();

            var workspace = await repo.GetByIdAsync(request.WorkspaceId);
            if (workspace == null)
                return Result.Failure("Workspace không tồn tại.");

            await _permission.Workspace.EnsureAdminAsync(workspace.Id, userId);

            workspace.Type = request.Type;

            await repo.UpdateAsync(workspace);
            await _unitOfWork.SaveChangesAsync();

            await _realtime.SendToWorkspaceAsync(
                workspace.Id,
                WorkspaceEvents.Updated,
                new
                {
                    WorkspaceId = workspace.Id,
                    Type = workspace.Type
                }
            );

            return Result.Success();
        }
    }
}
