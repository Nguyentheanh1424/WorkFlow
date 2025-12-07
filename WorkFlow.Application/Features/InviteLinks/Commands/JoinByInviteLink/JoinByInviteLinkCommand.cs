using MediatR;
using WorkFlow.Domain.Common;

namespace WorkFlow.Application.Features.InviteLinks.Commands.JoinByInviteLink
{
    public record JoinByInviteLinkCommand(string Token)
        : IRequest<Result<bool>>;
}
