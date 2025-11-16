using WorkFlow.Application.Common.Cache;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Common.Helpers;

namespace WorkFlow.Infrastructure.Services
{
    public class OtpService : IOtpService
    {
        private readonly ICacheService _cache;

        public OtpService(ICacheService cache)
        {
            _cache = cache;
        }

        private static string BuildCacheKey(string key)
            => $"otp:{key}";

        public async Task<string> GenerateAsync(string key, int length = 6)
        {
            var otp = OtpGenerator.GenerateNumeric(length);

            // TTL OTP 2 phút
            var ttl = TimeSpan.FromMinutes(2);

            var model = new OtpCacheModel(key, otp, ttl);

            await _cache.SetAsync(model, absoluteTtl: ttl);

            return otp;
        }

        public async Task<bool> VerifyAsync(string key, string otp)
        {
            var cacheKey = BuildCacheKey(key);

            var model = await _cache.GetAsync<OtpCacheModel>(cacheKey);
            if (model == null)
                return false;

            if (model.Otp != otp)
                return false;

            // Dùng xong xoá luôn
            await _cache.RemoveAsync(cacheKey);

            return true;
        }
    }
}
