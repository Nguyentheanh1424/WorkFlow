using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Common.Helpers;
using WorkFlow.Domain.Entities;
using WorkFlow.Application.Common.Exceptions;

namespace WorkFlow.Application.Features.Authentication.Commands;

public record ForgotPasswordVerifyCommand(string Email, string Otp, string NewPassword)
    : IRequest<Result>;

public class ForgotPasswordVerifyValidator : AbstractValidator<ForgotPasswordVerifyCommand>
{
    public ForgotPasswordVerifyValidator()
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

public class ForgotPasswordVerifyHandler
    : IRequestHandler<ForgotPasswordVerifyCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly IRepository<User, Guid> _userRepo;
    private readonly IRepository<AccountAuth, Guid> _authRepo;
    private readonly IOtpService _otp;

    public ForgotPasswordVerifyHandler(
        IUnitOfWork uow,
        IOtpService otp)
    {
        _uow = uow;
        _otp = otp;

        _userRepo = _uow.GetRepository<User, Guid>();
        _authRepo = _uow.GetRepository<AccountAuth, Guid>();
    }

    public async Task<Result> Handle(ForgotPasswordVerifyCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.FirstOrDefaultAsync(x => x.Email == request.Email)
            ?? throw new NotFoundException("Email không tồn tại.");

        var account = await _authRepo
            .FirstOrDefaultAsync(a => a.UserId == user.Id && a.Provider == "Local")
            ?? throw new NotFoundException("Tài khoản chưa thiết lập đăng nhập Local.");

        var isValid = await _otp.VerifyAsync(user.Id.ToString(), request.Otp);
        if (!isValid)
            return Result.Failure("OTP không hợp lệ hoặc đã hết hạn.");

        var (hash, salt) = PasswordHasher.Hash(request.NewPassword);
        account.SetPassword(hash, salt);

        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Success("Khôi phục mật khẩu thành công.");
    }
}
