using FluentValidation;

namespace WorkFlow.Application.Features.InviteLinks.Queries
{
    public class GetInviteLinkQueryValidator : AbstractValidator<GetInviteLinkQuery>
    {
        public GetInviteLinkQueryValidator()
        {
            RuleFor(x => x.Token).NotEmpty().WithMessage("Token không được để trống.");
        }
    }
}
