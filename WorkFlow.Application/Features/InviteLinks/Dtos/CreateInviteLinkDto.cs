using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.InviteLinks.Dtos
{
    public class CreateInviteLinkDto
    {
        public InviteLinkType Type { get; set; }
        public Guid TargetId { get; set; } // workspaceId hoặc boardId
        public DateTime? ExpiredAt { get; set; }
        public int MaxUses { get; set; } = 1;
    }
}
