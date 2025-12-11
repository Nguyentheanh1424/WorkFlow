using MediatR;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;
using WorkFlow.Application.Common.Exceptions;

namespace WorkFlow.Application.Features.Authentication.Commands;
public record ResetPasswordSendOtpCommand() : IRequest<Result>;

public class ResetPasswordSendOtpHandler
    : IRequestHandler<ResetPasswordSendOtpCommand, Result>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _uow;
    private readonly IRepository<User, Guid> _userRepo;
    private readonly IRepository<AccountAuth, Guid> _authRepo;
    private readonly IOtpService _otp;
    private readonly IEmailService _email;

    public ResetPasswordSendOtpHandler(
        ICurrentUserService currentUserService,
        IUnitOfWork uow,
        IOtpService otp,
        IEmailService email)
    {
        _currentUserService = currentUserService;
        _uow = uow;
        _otp = otp;
        _email = email;

        _userRepo = _uow.GetRepository<User, Guid>();
        _authRepo = _uow.GetRepository<AccountAuth, Guid>();
    }

    public async Task<Result> Handle(ResetPasswordSendOtpCommand request, CancellationToken ct)
    {
        if (_currentUserService.UserId == null)
            return Result.Failure("Không xác định được người dùng.");

        var user = await _userRepo.GetByIdAsync(_currentUserService.UserId.Value)
            ?? throw new NotFoundException("Người dùng không tồn tại.");

        var account = await _authRepo
            .FirstOrDefaultAsync(a => a.UserId == user.Id && a.Provider == "Local")
            ?? throw new NotFoundException("Tài khoản chưa thiết lập phương thức đăng nhập Local.");

        var otp = await _otp.GenerateAsync(user.Id.ToString());
        await _email.SendAsync(user.Email, "Mã OTP đặt lại mật khẩu", $"OTP của bạn là: {otp}");

        return Result.Success("OTP đã được gửi đến email.");
    }
}
