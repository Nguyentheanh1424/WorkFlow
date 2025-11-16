namespace WorkFlow.Application.Common.Interfaces.Services
{
    public interface IOtpService
    {
        Task<string> GenerateAsync(string key, int length = 6);
        Task<bool> VerifyAsync(string key, string otp);
    }
}
