using MediatR;
using WorkFlow.Domain.Common;

namespace WorkFlow.Application.Features.InviteLinks.Commands.RevokeInviteLink
{
    public record RevokeInviteLinkCommand(Guid Id)
        : IRequest<Result<bool>>;
}
