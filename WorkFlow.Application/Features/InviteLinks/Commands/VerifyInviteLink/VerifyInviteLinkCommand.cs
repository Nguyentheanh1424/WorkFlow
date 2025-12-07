using MediatR;
using WorkFlow.Domain.Common;

namespace WorkFlow.Application.Features.InviteLinks.Commands.VerifyInviteLink
{
    public record VerifyInviteLinkCommand(string Token)
        : IRequest<Result<bool>>;
}
