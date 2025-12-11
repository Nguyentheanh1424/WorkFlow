using MediatR;
using WorkFlow.Domain.Common;

namespace WorkFlow.Application.Features.InviteLinks.Queries
{
    public record GetInviteLinkQuery(string Token)
        : IRequest<Result<InviteLinkDto>>;
}
