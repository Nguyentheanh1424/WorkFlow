using FluentValidation;

namespace WorkFlow.Application.Features.InviteLinks.Commands.RevokeInviteLink
{
    public class RevokeInviteLinkCommandValidator : AbstractValidator<RevokeInviteLinkCommand>
    {
        public RevokeInviteLinkCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
