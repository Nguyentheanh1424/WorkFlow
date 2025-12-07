using FluentValidation;

namespace WorkFlow.Application.Features.InviteLinks.Commands.VerifyInviteLink
{
    public class VerifyInviteLinkCommandValidator : AbstractValidator<VerifyInviteLinkCommand>
    {
        public VerifyInviteLinkCommandValidator()
        {
            RuleFor(x => x.Token).NotEmpty();
        }
    }
}
