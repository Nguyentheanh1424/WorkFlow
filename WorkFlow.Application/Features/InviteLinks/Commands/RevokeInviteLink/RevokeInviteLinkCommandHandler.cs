using MediatR;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.InviteLinks.Commands.RevokeInviteLink
{
    public class RevokeInviteLinkCommandHandler
        : IRequestHandler<RevokeInviteLinkCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<InviteLink, Guid> _repository;

        public RevokeInviteLinkCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _repository = unitOfWork.GetRepository<InviteLink, Guid>();
        }

        public async Task<Result<bool>> Handle(RevokeInviteLinkCommand request, CancellationToken cancellationToken)
        {
            var link = await _repository.GetByIdAsync(request.Id);

            if (link == null)
                return Result<bool>.Failure("Không tìm thấy link.");

            link.Revoke();

            await _repository.UpdateAsync(link);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}
