using FluentValidation;
using WorkFlow.Application.Features.InviteLinks.Dtos;

namespace WorkFlow.Application.Features.InviteLinks.Commands.CreateInviteLink
{
    public class CreateInviteLinkCommandValidator : AbstractValidator<CreateInviteLinkCommand>
    {
        public CreateInviteLinkCommandValidator()
        {
            RuleFor(x => x.Request).NotNull();
            RuleFor(x => x.Request.Type).IsInEnum();
            RuleFor(x => x.Request.TargetId).NotEmpty();
        }
    }
}
