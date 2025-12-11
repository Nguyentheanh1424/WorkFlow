using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.InviteLinks.Commands
{
    public record JoinByInviteLinkCommand(string Token) : IRequest<Result<bool>>;
    public class JoinByInviteLinkCommandValidator : AbstractValidator<JoinByInviteLinkCommand>
    {
        public JoinByInviteLinkCommandValidator()
        {
            RuleFor(x => x.Token).NotEmpty();
        }
    }
    public class JoinByInviteLinkCommandHandler : IRequestHandler<JoinByInviteLinkCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<InviteLink, Guid> _repository;

        public JoinByInviteLinkCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _repository = unitOfWork.GetRepository<InviteLink, Guid>();
        }

        public async Task<Result<bool>> Handle(JoinByInviteLinkCommand request, CancellationToken cancellationToken)
        {
            var links = await _repository.FindAsync(x => x.Token == request.Token);

            if (!links.Any())
                return Result<bool>.Failure("Link không tồn tại.");

            var link = links.First();

            // Check and update expire status if needed
            if (link.CheckAndUpdateExpireStatus())
            {
                await _repository.UpdateAsync(link);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result<bool>.Failure("Link đã hết hạn.");
            }

            if (link.Status == InviteLinkStatus.Revoked)
                return Result<bool>.Failure("Link đã bị thu hồi.");

            if (link.Status == InviteLinkStatus.Expired)
                return Result<bool>.Failure("Link đã hết hạn.");

            await _repository.UpdateAsync(link);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // TODO: Add user to workSpace/board

            return Result<bool>.Success(true);
        }
    }
}
