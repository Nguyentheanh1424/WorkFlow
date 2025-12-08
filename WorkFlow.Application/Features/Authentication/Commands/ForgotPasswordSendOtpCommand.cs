using MediatR;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;
using WorkFlow.Application.Common.Exceptions;

namespace WorkFlow.Application.Features.Authentication.Commands;

public record ForgotPasswordSendOtpCommand(string Email)
    : IRequest<Result>;

public class ForgotPasswordSendOtpHandler
    : IRequestHandler<ForgotPasswordSendOtpCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly IRepository<User, Guid> _userRepo;
    private readonly IRepository<AccountAuth, Guid> _authRepo;
    private readonly IOtpService _otp;
    private readonly IEmailService _email;

    public ForgotPasswordSendOtpHandler(
        IUnitOfWork uow,
        IOtpService otp,
        IEmailService email)
    {
        _uow = uow;
        _otp = otp;
        _email = email;

        _userRepo = _uow.GetRepository<User, Guid>();
        _authRepo = _uow.GetRepository<AccountAuth, Guid>();
    }

    public async Task<Result> Handle(ForgotPasswordSendOtpCommand request, CancellationToken ct)
    {
        var user = await _userRepo.FirstOrDefaultAsync(x => x.Email == request.Email)
            ?? throw new NotFoundException("Email không tồn tại.");

        var account = await _authRepo
            .FirstOrDefaultAsync(a => a.UserId == user.Id && a.Provider == "Local")
            ?? throw new NotFoundException("Tài khoản chưa thiết lập đăng nhập Local.");

        var otp = await _otp.GenerateAsync(user.Id.ToString());

        await _email.SendAsync(user.Email, "OTP đặt lại mật khẩu", $"OTP của bạn: {otp}");

        return Result.Success("OTP đã được gửi.");
    }
}
