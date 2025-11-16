using System;
using System.ComponentModel.DataAnnotations.Schema;
using WorkFlow.Domain.Common;

namespace WorkFlow.Domain.Entities
{
    public class AccountAuth<TId> : AuditableEntity<TId>
    {
        [Column("UserId")]
        public TId UserId { get; private set; } = default!;

        [Column("Provider")]
        public string Provider { get; private set; } = "local";

        [Column("ProviderUid")]
        public string ProviderUid { get; private set; } = string.Empty;

        [Column("PasswordHash")]
        public string HashPassword { get; private set; } = string.Empty;

        [Column("Salt")]
        public string Salt { get; private set; } = string.Empty;

        [Column("AccessToken")]
        public string AccessToken { get; private set; } = string.Empty;

        [Column("RefreshToken")]
        public string RefreshToken { get; private set; } = string.Empty;

        private DateTime _lastLoginAt = DateTime.UtcNow;
        [Column("LastLoginAt")]
        public DateTime LastLoginAt
        {
            get => _lastLoginAt;
            private set => _lastLoginAt = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        private DateTime _lastPasswordChange = DateTime.UtcNow;
        [Column("LastPasswordChange")]
        public DateTime LastPasswordChange
        {
            get => _lastPasswordChange;
            private set => _lastPasswordChange = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        [Column("LoginAttemp")]
        public int LoginAttempt { get; private set; } = 0;

        private DateTime? _lockedUntil;
        [Column("LockedUntil")]
        public DateTime? LockedUntil
        {
            get => _lockedUntil;
            private set => _lockedUntil = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
        }

        public AccountAuth(TId userId)
        {
            UserId = userId;
        }

        public void SetProviderUid(string email) => ProviderUid = email;

        public void SetPassword(string hash)
        {
            HashPassword = hash;
            LastPasswordChange = DateTime.UtcNow;
        }

        public void UpdateLastLogin() => LastLoginAt = DateTime.UtcNow;

        public void SetAccessToken(string token) => AccessToken = token;

        public void IncrementLoginAttempt() => LoginAttempt++;

        public void LockUntil(DateTime untilUtc) => LockedUntil = DateTime.SpecifyKind(untilUtc, DateTimeKind.Utc);
    }
}
