using FluentValidation;

namespace WorkFlow.Application.Features.InviteLinks.Commands.JoinByInviteLink
{
    public class JoinByInviteLinkCommandValidator : AbstractValidator<JoinByInviteLinkCommand>
    {
        public JoinByInviteLinkCommandValidator()
        {
            RuleFor(x => x.Token).NotEmpty();
        }
    }
}
