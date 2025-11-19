namespace WorkFlow.Application.Common.Cache
{
    public class OtpAttemptCacheModel : CacheModelBase
    {
        public int AttemptCount { get; set; } = 0;
        public DateTime NextAvailableAt { get; set; } = DateTime.MinValue;

        public OtpAttemptCacheModel() { }

        public OtpAttemptCacheModel(int attemptCount, DateTime nextAvailableAt, string key, TimeSpan ttl)
            : base($"otp-attempt:{key}", absoluteTtl: ttl)
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
