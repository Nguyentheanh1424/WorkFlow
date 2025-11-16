using MediatR;
using WorkFlow.Application.Features.Workflows.Dtos;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Common.Cache;
using System.Security.Cryptography;


namespace WorkFlow.Application.Features.Workflows.Commands
{public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, bool>
{
    private readonly ICacheService _cache;
    private readonly IEmailService _email;
    private readonly TimeSpan _otpTtl = TimeSpan.FromMinutes(5);
    private readonly int _maxOtpAttempts = 3;

    public RegisterUserCommandHandler(ICacheService cache, IEmailService email)
    {
        _cache = cache;
        _email = email;
    }

    public async Task<bool> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        var cacheKey = $"PENDING_USER:{request.Dto.Email}";

        var existing = await _cache.GetAsync<PendingUserCacheModel>(cacheKey);

        if (existing != null && existing.Attempts >= _maxOtpAttempts)
            throw new Exception("Bạn đã gửi OTP quá số lần cho phép. Vui lòng thử lại sau.");

        var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        Console.WriteLine($"[DEBUG] OTP for {request.Dto.Email}: {otp}");
        var model = new PendingUserCacheModel(cacheKey)
        {
            Email = request.Dto.Email,
            PlainPassword = request.Dto.Password,
            Name = request.Dto.Name,
            Otp = otp,
            Attempts = existing != null ? existing.Attempts + 1 : 1
        };

        await _cache.SetAsync(model, absoluteTtl: _otpTtl, slidingTtl: _otpTtl);

        await _email.SendAsync(request.Dto.Email, "OTP đăng ký", $"Mã OTP của bạn: {otp}");

        return true;
    }
}
}