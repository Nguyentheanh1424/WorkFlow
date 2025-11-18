using WorkFlow.Domain.Common;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Domain.Entities
{
    public class AccountAuth : AuditableEntity<Guid>
    {
        public Guid UserId { get; private set; } = default!;

        // Login provinder
        public string Provider { get; private set; } = EnumExtensions.GetName(AccountProvider.Local);
        public string ProviderUid { get; private set; } = string.Empty;

        // Password auth
        public string PasswordHash { get; private set; } = string.Empty;
        public string Salt { get; private set; } = string.Empty;

        // Account status
        public AccountStatus Status { get; private set; } = AccountStatus.Actived;

        // Refresh token
        public string RefreshToken { get; private set; } = string.Empty;
        public DateTime? RefreshTokenExpireAt { get; private set; }

        // Login security
        public DateTime LastLoginAt { get; private set; } = DateTime.UtcNow;
        public DateTime LastPasswordChangeAt { get; private set; } = DateTime.UtcNow;
        public int LoginAttempt { get; private set; } = 0;
        public DateTime? LockedUntil { get; private set; }

        protected AccountAuth() { }

        public static AccountAuth CreateLocal(Guid userId, string passwordHash, string salt)
        {
            return new AccountAuth
            {
                UserId = userId,
                PasswordHash = passwordHash,
                Salt = salt,
            };
        }

        public static AccountAuth CreateOAuth(Guid userId, string provider, string providerUid)
        {
            return new AccountAuth
            {
                UserId = userId,
                Provider = provider,
                ProviderUid = providerUid,
            };
        }

        public void SetPassword(string hash, string salt)
        {
            PasswordHash = hash;
            Salt = salt;
            LastPasswordChangeAt = DateTime.UtcNow;
        }

        public void SetRefreshToken(string refreshToken, int days)
        {
            RefreshToken = refreshToken;
            RefreshTokenExpireAt = DateTime.UtcNow.AddDays(days);
        }

        public void RevokeRefreshToken()
        {
            RefreshToken = string.Empty;
            RefreshTokenExpireAt = null;
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

        public void SetStatus(AccountStatus status)
        {
            Status = status;
        }
    }
}
