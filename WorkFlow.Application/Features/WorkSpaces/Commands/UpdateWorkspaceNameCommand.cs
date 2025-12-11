using AutoMapper;
using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Constants.EventNames;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.WorkSpaces.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.WorkSpaces.Commands
{
    public record UpdateWorkspaceNameCommand(Guid WorkspaceId, string Name)
    : IRequest<Result>;

    public class UpdateWorkspaceNameCommandValidator : AbstractValidator<UpdateWorkspaceNameCommand>
    {
        public UpdateWorkspaceNameCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên không được để trống.")
                .MaximumLength(255).WithMessage("Tên không được vượt quá 255 ký tự.");
        }
    }

    public class UpdateWorkspaceNameCommandHandler
    : IRequestHandler<UpdateWorkspaceNameCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IPermissionService _permission;
        private readonly IRealtimeService _realtime;

        public UpdateWorkspaceNameCommandHandler(
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

        public async Task<Result> Handle(UpdateWorkspaceNameCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result.Failure("Không xác định người dùng.");

            var userId = _currentUser.UserId.Value;

            var repo = _unitOfWork.GetRepository<WorkSpace, Guid>();

            var workspace = await repo.GetByIdAsync(request.WorkspaceId);
            if (workspace == null)
                return Result.Failure("Workspace không tồn tại.");

            await _permission.Workspace.EnsureAdminAsync(workspace.Id, userId);

            workspace.Name = request.Name;

            await repo.UpdateAsync(workspace);
            await _unitOfWork.SaveChangesAsync();

            await _realtime.SendToWorkspaceAsync(
                workspace.Id,
                WorkspaceEvents.Updated,
                new
                {
                    WorkspaceId = workspace.Id,
                    Name = workspace.Name
                }
            );

            return Result.Success();
        }
    }
}
