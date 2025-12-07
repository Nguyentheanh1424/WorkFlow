using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.InviteLinks.Commands
{
    public record RevokeInviteLinkCommand(Guid tagetId) : IRequest<Result<bool>>;
    public class RevokeInviteLinkCommandValidator : AbstractValidator<RevokeInviteLinkCommand>
    {
        public RevokeInviteLinkCommandValidator()
        {
            RuleFor(x => x.tagetId).NotEmpty();
        }
    }
    public class RevokeInviteLinkCommandHandler : IRequestHandler<RevokeInviteLinkCommand, Result<bool>>
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
            var link = await _repository.GetByIdAsync(request.tagetId)
                ?? throw new NotFoundException("Link không tồn tại");

            link.Revoke();

            await _repository.UpdateAsync(link);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
