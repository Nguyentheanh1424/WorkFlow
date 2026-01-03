using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.WorkSpaces.Commands
{
    public record DeleteWorkspaceCommand(Guid Id) : IRequest<Result<bool>>;

    public class DeleteWorkspaceCommandValidator : AbstractValidator<DeleteWorkspaceCommand>
    {
        public DeleteWorkspaceCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    public class DeleteWorkspaceCommandHandler : IRequestHandler<DeleteWorkspaceCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<WorkSpace, Guid> _repository;
        private readonly ICurrentUserService _currentUser;
        private readonly IPermissionService _permission;

        public DeleteWorkspaceCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IPermissionService permissionService)
        {
            _unitOfWork = unitOfWork;
            _permission = permissionService;
            _currentUser = currentUserService;
            _repository = _unitOfWork.GetRepository<WorkSpace, Guid>();
        }

        public async Task<Result<bool>> Handle(DeleteWorkspaceCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<bool>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var workSpace = await _repository.GetByIdAsync(request.Id);
            if (workSpace == null)
                return Result<bool>.Failure("Workspace không tồn tại.");

            await _permission.Workspace.EnsureOwnerAsync(workSpace.Id, userId);

            await _repository.DeleteAsync(workSpace);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
