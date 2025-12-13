using WorkFlow.Application.Common.Mappings;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.InviteLinks.Dtos
{
    public class InviteLinkDto : IMapFrom<InviteLink>
    {
        public Guid Id { get; set; }
        public InviteLinkType Type { get; set; }
        public Guid TargetId { get; set; }

        public Guid? InvitedUserId { get; set; }

        public string Slug { get; set; } = null!;
        public string Token { get; set; } = null!;
        public InviteLinkStatus Status { get; set; }
        public InviteLinkExpireReason? ExpireReason { get; set; }

        public DateTime? ExpiredAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
