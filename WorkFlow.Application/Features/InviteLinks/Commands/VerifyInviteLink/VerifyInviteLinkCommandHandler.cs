using MediatR;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.InviteLinks.Commands.VerifyInviteLink
{
    public class VerifyInviteLinkCommandHandler
        : IRequestHandler<VerifyInviteLinkCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<InviteLink, Guid> _repository;

        public VerifyInviteLinkCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _repository = unitOfWork.GetRepository<InviteLink, Guid>();
        }

        public async Task<Result<bool>> Handle(VerifyInviteLinkCommand request, CancellationToken cancellationToken)
        {
            var links = await _repository.FindAsync(x => x.Token == request.Token);

            if (!links.Any())
                return Result<bool>.Failure("Link không tồn tại.");

            var link = links.First();

            // Check and update expire status if needed
            if (link.CheckAndUpdateExpireStatus())
            {
                await _repository.UpdateAsync(link);
                await _unitOfWork.SaveChangesAsync();
                return Result<bool>.Failure("Link đã hết hạn.");
            }

            if (link.Status == InviteLinkStatus.Revoked)
                return Result<bool>.Failure("Link đã bị thu hồi.");

            if (link.Status == InviteLinkStatus.Expired)
                return Result<bool>.Failure("Link đã hết hạn.");

            return Result<bool>.Success(true);
        }
    }
}
