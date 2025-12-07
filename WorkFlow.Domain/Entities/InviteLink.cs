using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Domain.Entities
{
    public class InviteLink : FullAuditEntity<Guid>
    {
        public InviteLinkType Type { get; set; }
        public string Token { get; set; } = null!;
        public InviteLinkStatus Status { get; set; }
        public DateTime? ExpiredAt { get; set; }

        public Guid? WorkSpaceId { get; set; }
        public Guid? BoardId { get; set; }

        protected InviteLink() { }

        public static InviteLink Create(InviteLinkType type, string token, Guid? workSpaceId, Guid? boardId, DateTime? expiredAt)
        {
            return new InviteLink
            {
                Type = type,
                Token = token,
                WorkSpaceId = workSpaceId,
                BoardId = boardId,
                ExpiredAt = expiredAt,
                Status = InviteLinkStatus.Active
            };
        }

        public void Revoke()
        {
            Status = InviteLinkStatus.Revoked;
        }

        public void Expire()
        {
            Status = InviteLinkStatus.Expired;
        }

        public void UpdateExpire(DateTime? newExpireAt)
        {
            ExpiredAt = newExpireAt;
        }

        public bool CheckAndUpdateExpireStatus()
        {
            if (Status == InviteLinkStatus.Expired || Status == InviteLinkStatus.Revoked)
                return false;

            if (ExpiredAt.HasValue && ExpiredAt < DateTime.UtcNow)
            {
                Expire();
                return true;
            }

            return false;
        }

        public bool IsActive() => Status == InviteLinkStatus.Active && (ExpiredAt == null || ExpiredAt > DateTime.UtcNow);
    }
}
