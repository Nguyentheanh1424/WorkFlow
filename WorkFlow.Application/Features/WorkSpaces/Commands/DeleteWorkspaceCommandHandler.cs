using MediatR;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.WorkSpaces.Commands
{
    public class DeleteWorkspaceCommandHandler : IRequestHandler<DeleteWorkspaceCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<WorkSpace, Guid> _repository;

        public DeleteWorkspaceCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _repository = _unitOfWork.GetRepository<WorkSpace, Guid>();
        }

        public async Task<Result<bool>> Handle(DeleteWorkspaceCommand request, CancellationToken cancellationToken)
        {
            var workSpace = await _repository.GetByIdAsync(request.Id);
            if (workSpace == null)
                return Result<bool>.Failure("Workspace không tồn tại.");

            // Soft delete - mark as deleted by setting DeletedOn timestamp
            // This assumes the entity uses soft delete from FullAuditEntity
            await _repository.DeleteAsync(workSpace);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
