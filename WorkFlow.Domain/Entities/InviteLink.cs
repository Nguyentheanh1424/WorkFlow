using WorkFlow.Domain.Common;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Domain.Entities
{
    public class InviteLink : FullAuditEntity<Guid>
    {
        public InviteLinkType Type { get; private set; }
        public Guid TargetId { get; private set; }

        public Guid? InvitedUserId { get; private set; }

        public string Slug { get; private set; } = null!;
        public string Token { get; private set; } = null!;

        public InviteLinkStatus Status { get; private set; }
        public InviteLinkExpireReason? ExpireReason { get; private set; }

        public DateTime? ExpiredAt { get; private set; }

        protected InviteLink() { }

        public static InviteLink Create(
            InviteLinkType type,
            Guid targetId,
            Guid? invitedUserId,
            string? slug,
            DateTime? expiredAt)
        {
            return new InviteLink
            {
                Id = Guid.NewGuid(),
                Type = type,
                TargetId = targetId,
                InvitedUserId = invitedUserId,
                Slug = string.IsNullOrWhiteSpace(slug)
                    ? GenerateSlug()
                    : slug,
                Token = GenerateToken(),
                ExpiredAt = expiredAt,
                Status = InviteLinkStatus.Active
            };
        }

        public bool IsExpiredByTime()
            => ExpiredAt.HasValue && ExpiredAt < DateTime.UtcNow;

        public bool IsActive()
        {
            if (Status != InviteLinkStatus.Active)
                return false;

            if (IsExpiredByTime())
                return false;

            return true;
        }

        public void EnsureCanBeUsedBy(Guid userId)
        {
            if (InvitedUserId.HasValue && InvitedUserId != userId)
                throw new DomainException("Invite link này không dành cho bạn.");
        }

        public void Revoke()
        {
            if (Status != InviteLinkStatus.Active)
                return;

            Status = InviteLinkStatus.Revoked;
        }

        public void CheckAndExpireByTime()
        {
            if (Status != InviteLinkStatus.Active)
                return;

            if (IsExpiredByTime())
                Expire(InviteLinkExpireReason.TimeExpired);
        }

        private void Expire(InviteLinkExpireReason reason)
        {
            Status = InviteLinkStatus.Expired;
            ExpireReason = reason;
        }

        private static string GenerateToken()
            => $"{Guid.NewGuid():N}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

        private static string GenerateSlug()
            => Guid.NewGuid().ToString("N")[..8];
    }

}
