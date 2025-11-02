using WorkFlow.Domain.Common;

namespace WorkFlow.Domain.Entities
{
    public class AccountAuth<Guid> : AuditableEntity<Guid>
    {
        public Guid UserId { get; private set; } = default!;
        public string Provider { get; private set; } = "local";
        public string ProvinderUId { get; private set; } = string.Empty;
        public string HashPassword { get; private set; } = string.Empty;
        public string Salt { get; private set; } = string.Empty;
        public string AccessToken { get; private set; } = string.Empty;
        public string RefreshToken { get; private set; } = string.Empty;
        public DateTime LastLoginAt { get; private set; } = DateTime.UtcNow;
        public DateTime LastPasswordChangeAt { get; private set; } = DateTime.UtcNow;
        public int LoginAttempt { get; private set; } = 0;
        public DateTime LockedUntil { get; private set; }
    }
}
