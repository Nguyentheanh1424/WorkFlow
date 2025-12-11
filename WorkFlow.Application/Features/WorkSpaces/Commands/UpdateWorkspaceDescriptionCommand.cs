using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Constants.EventNames;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.WorkSpaces.Commands
{
    public record UpdateWorkspaceDescriptionCommand(Guid WorkspaceId, string? Description)
    : IRequest<Result>;


    public class UpdateWorkspaceDescriptionCommandValidator : AbstractValidator<UpdateWorkspaceDescriptionCommand>
    {
        public UpdateWorkspaceDescriptionCommandValidator()
        {
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description không được vượt quá 1000 ký tự.");
        }
    }

    public class UpdateWorkspaceDescriptionCommandHandler
    : IRequestHandler<UpdateWorkspaceDescriptionCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IPermissionService _permission;
        private readonly IRealtimeService _realtime;

        public UpdateWorkspaceDescriptionCommandHandler(
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

        public async Task<Result> Handle(UpdateWorkspaceDescriptionCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result.Failure("Không xác định người dùng.");

            var userId = _currentUser.UserId.Value;

            var repo = _unitOfWork.GetRepository<WorkSpace, Guid>();

            var workspace = await repo.GetByIdAsync(request.WorkspaceId);
            if (workspace == null)
                return Result.Failure("Workspace không tồn tại.");

            await _permission.Workspace.EnsureAdminAsync(workspace.Id, userId);

            workspace.Description = request.Description;

            await repo.UpdateAsync(workspace);
            await _unitOfWork.SaveChangesAsync();

            await _realtime.SendToWorkspaceAsync(
                workspace.Id,
                WorkspaceEvents.Updated,
                new
                {
                    WorkspaceId = workspace.Id,
                    Description = workspace.Description
                }
            );

            return Result.Success();
        }
    }
}
