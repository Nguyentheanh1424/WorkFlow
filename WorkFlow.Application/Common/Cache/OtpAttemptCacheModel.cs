namespace WorkFlow.Application.Common.Cache
{
    public class OtpAttemptCacheModel : CacheModelBase
    {
        public int AttemptCount { get; private set; }
        public DateTime NextAvailableAt { get; private set; }

        public OtpAttemptCacheModel() : base("") { }

        public OtpAttemptCacheModel(string email, int attemptCount, DateTime nextAvailableAt, TimeSpan ttl)
            : base($"otp-attempt:{email}", absoluteTtl: ttl)
        {
            AttemptCount = attemptCount;
            NextAvailableAt = nextAvailableAt;
        }

        public void Increase() => AttemptCount++;

        public void SetCooldown(TimeSpan cooldown)
        {
            NextAvailableAt = DateTime.UtcNow.Add(cooldown);
        }
    }
}
