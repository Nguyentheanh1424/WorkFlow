using MediatR;
using WorkFlow.Application.Features.InviteLinks.Dtos;
using WorkFlow.Domain.Common;

namespace WorkFlow.Application.Features.InviteLinks.Commands.CreateInviteLink
{
    public record CreateInviteLinkCommand(CreateInviteLinkDto Request)
        : IRequest<Result<InviteLinkDto>>;
}
