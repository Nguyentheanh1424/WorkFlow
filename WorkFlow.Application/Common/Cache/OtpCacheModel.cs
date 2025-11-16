namespace WorkFlow.Application.Common.Cache
{
    public class OtpCacheModel : CacheModelBase
    {
        public string Key { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;

        public OtpCacheModel(string key, string otp, TimeSpan ttl)
        : base($"otp:{key}", absoluteTtl: ttl)
        {
            Otp = otp;
            Key = key;
        }
    }
}
