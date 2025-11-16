using WorkFlow.Domain.Common;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Domain.Entities
{
    public class AccountAuth : AuditableEntity<Guid>
    {
        public Guid UserId { get; private set; } = default!;

        // Login provinder
        public string Provider { get; private set; } = "local";
        public string ProvinderUid { get; private set; } = string.Empty;

        // Password auth
        public string PasswordHash { get; private set; } = string.Empty;
        public string Salt { get; private set; } = string.Empty;

        // Account status
        public AccountStatus Status { get; private set; } = AccountStatus.Actived;

        // Refresh token
        public string RefreshTokenHash { get; private set; } = string.Empty;
        public DateTime? RefreshTokenExpireAt { get; private set; }

        // Login security
        public DateTime LastLoginAt { get; private set; } = DateTime.UtcNow;
        public DateTime LastPasswordChangeAt { get; private set; } = DateTime.UtcNow;
        public int LoginAttempt { get; private set; } = 0;
        public DateTime? LockedUntil { get; private set; }

        public AccountAuth(Guid userId, string passwordHash, string salt)
        {
            UserId = userId;
            Provider = passwordHash;
            Provider = salt;
        }

        public void SetPassword(string hash, string salt)
        {
            PasswordHash = hash;
            Salt = salt;
            LastPasswordChangeAt = DateTime.UtcNow;
        }

        public void SetRefreshToken(string refreshToken, DateTime expire)
        {
            RefreshTokenHash = refreshToken;
            RefreshTokenExpireAt = expire;
        }

        public void MarkLoginSuccess()
        {
            LoginAttempt = 0;
            LastLoginAt = DateTime.UtcNow;
        }

        public void MarkLoginFailed(int maxFail = 5, int lockMinutes = 30)
        {
            LoginAttempt++;

            if (LoginAttempt >= maxFail)
            {
                LockedUntil = DateTime.UtcNow.AddMinutes(lockMinutes);
                LoginAttempt = 0;
            }
        }

        public bool IsLocked()
        {
            return LockedUntil.HasValue && DateTime.UtcNow < LockedUntil.Value;
        }
    }
}
