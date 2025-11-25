using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Common.Helpers;
using WorkFlow.Domain.Entities;
using WorkFlow.Application.Common.Exceptions;

namespace WorkFlow.Application.Features.Authentication.Commands;
public record ResetPasswordVerifyCommand(string Otp, string NewPassword)
    : IRequest<Result<string>>;

public class ResetPasswordVerifyValidator : AbstractValidator<ResetPasswordVerifyCommand>
{
    public ResetPasswordVerifyValidator()
    {
        RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Mật khẩu không được để trống.")
                .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 ký tự.")
                .Matches("[A-Z]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ hoa (A-Z).")
                .Matches("[a-z]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ thường (a-z).")
                .Matches("[0-9]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ số (0-9).")
                .Matches("[^a-zA-Z0-9]").WithMessage("Mật khẩu phải chứa ít nhất 1 ký tự đặc biệt.");
    }
}

public class ResetPasswordVerifyHandler
    : IRequestHandler<ResetPasswordVerifyCommand, Result<string>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _uow;
    private readonly IRepository<User, Guid> _userRepo;
    private readonly IRepository<AccountAuth, Guid> _authRepo;
    private readonly IOtpService _otp;

    public ResetPasswordVerifyHandler(
        ICurrentUserService currentUserService,
        IUnitOfWork uow,
        IOtpService otp)
    {
        _currentUserService = currentUserService;
        _uow = uow;
        _otp = otp;

        _userRepo = _uow.GetRepository<User, Guid>();
        _authRepo = _uow.GetRepository<AccountAuth, Guid>();
    }

    public async Task<Result<string>> Handle(ResetPasswordVerifyCommand request, CancellationToken ct)
    {
        if (_currentUserService.UserId == null)
            return Result<string>.Failure("Không xác định được người dùng.");

        var user = await _userRepo.GetByIdAsync(_currentUserService.UserId.Value)
            ?? throw new NotFoundException("Người dùng không tồn tại.");

        var account = await _authRepo
            .FirstOrDefaultAsync(a => a.UserId == user.Id && a.Provider == "Local")
            ?? throw new NotFoundException("Tài khoản chưa thiết lập đăng nhập Local.");

        bool isSame = PasswordHasher.Verify(request.NewPassword, account.PasswordHash, account.Salt);
        if (isSame)
            return Result<string>.Failure("Mật khẩu mới không được trùng mật khẩu cũ.");

        var isValid = await _otp.VerifyAsync(user.Id.ToString(), request.Otp);
        if (!isValid)
            return Result<string>.Failure("OTP không hợp lệ hoặc đã hết hạn.");

        var (hash, salt) = PasswordHasher.Hash(request.NewPassword);
        account.SetPassword(hash, salt);

        await _uow.SaveChangesAsync(ct);

        return Result<string>.Success("Đổi mật khẩu thành công.");
    }
}
