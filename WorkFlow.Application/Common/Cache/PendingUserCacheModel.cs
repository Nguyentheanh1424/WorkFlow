using System;

namespace WorkFlow.Application.Common.Cache
{
   public class PendingUserCacheModel : CacheModelBase
{
    public string Email { get; set; } = string.Empty;
    public string PlainPassword { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Attempts { get; set; } = 0; // số lần gửi OTP
    public PendingUserCacheModel(string cacheKey) : base(cacheKey) { }
}

}