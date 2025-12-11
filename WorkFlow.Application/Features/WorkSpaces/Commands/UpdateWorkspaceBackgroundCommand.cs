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

namespace WorkFlow.Application.Features.WorkSpaces.Commands
{
    public record UpdateWorkspaceBackgroundCommand(Guid WorkspaceId, string? Background)
    : IRequest<Result>;


    public class UpdateWorkspaceBackgroundCommandValidator
    : AbstractValidator<UpdateWorkspaceBackgroundCommand>
    {
        public UpdateWorkspaceBackgroundCommandValidator()
        {
            RuleFor(x => x.WorkspaceId).NotEmpty();
            RuleFor(x => x.Background)
                .MaximumLength(500)
                .WithMessage("Đường dẫn background quá dài.");
        }
    }

    public class UpdateWorkspaceBackgroundCommandHandler
    : IRequestHandler<UpdateWorkspaceBackgroundCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IPermissionService _permission;
        private readonly IRealtimeService _realtimeService;

        public UpdateWorkspaceBackgroundCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IPermissionService permission,
            IRealtimeService realtimeService)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _permission = permission;
            _realtimeService = realtimeService;
        }

        public async Task<Result> Handle(UpdateWorkspaceBackgroundCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result.Failure("Không xác định người dùng.");

            var userId = _currentUser.UserId.Value;

            var repo = _unitOfWork.GetRepository<WorkSpace, Guid>();

            var workspace = await repo.GetByIdAsync(request.WorkspaceId);
            if (workspace == null)
                return Result.Failure("Workspace không tồn tại.");

            await _permission.Workspace.EnsureAdminAsync(workspace.Id, userId);

            workspace.Background = request.Background;

            await repo.UpdateAsync(workspace);
            await _unitOfWork.SaveChangesAsync();

            await _realtimeService.SendToWorkspaceAsync(
                workspace.Id,
                WorkspaceEvents.Updated,
                new
                {
                    WorkspaceId = workspace.Id,
                    Background = workspace.Background
                }
            );

            return Result.Success();
        }
    }
}
