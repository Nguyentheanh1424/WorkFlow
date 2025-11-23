using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Common.Helpers;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Authentication.Commands
{
    public record ResetPasswordSendOtpCommand() : IRequest<Result<string>>;
    public record ResetPasswordVerifyCommand(string Otp, string NewPassword)
        : IRequest<Result<string>>;
    public class ResetPasswordVerifyCommandValidator : AbstractValidator<ResetPasswordVerifyCommand>
    {
        public ResetPasswordVerifyCommandValidator()
        {
            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(8)
                .Matches("[A-Z]").WithMessage("Mật khẩu phải có ít nhất 1 chữ hoa.")
                .Matches("[a-z]").WithMessage("Mật khẩu phải có ít nhất 1 chữ thường.")
                .Matches("[0-9]").WithMessage("Mật khẩu phải có ít nhất 1 số.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Mật khẩu phải có ít nhất 1 ký tự đặc biệt.");
        }
    }
    public class ResetPasswordSendOtpHandler
        : IRequestHandler<ResetPasswordSendOtpCommand, Result<string>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<User, Guid> _userRepository;
        private readonly IRepository<AccountAuth, Guid> _authRepository;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;

        public ResetPasswordSendOtpHandler(
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            IOtpService otpService,
            IEmailService emailService)
        {
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _otpService = otpService;
            _emailService = emailService;

            _authRepository = _unitOfWork.GetRepository<AccountAuth, Guid>();
            _userRepository = _unitOfWork.GetRepository<User, Guid>();
        }

        public async Task<Result<string>> Handle(ResetPasswordSendOtpCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedAccessException("Không xác định được người dùng.");

            var user = await _userRepository.GetByIdAsync(_currentUserService.UserId.Value)
                ?? throw new NotFoundException("Không xác định được người dùng.");

            var account = await _authRepository
                .FirstOrDefaultAsync(a => a.UserId == user.Id && a.Provider == "Local")
                ?? throw new NotFoundException("Tài khoản chưa thiết lập phương thức đăng nhập này.");

            var otp = await _otpService.GenerateAsync(user.Id.ToString());

            await _emailService.SendAsync(
                user.Email,
                "Mã OTP đặt lại mật khẩu",
                $"Mã OTP của bạn là: {otp}"
                );

            return Result<string>.Success("OTP đã được gửi đến email của bạn.");
        }
    }
    public class ResetPasswordVerifyHandler
        : IRequestHandler<ResetPasswordVerifyCommand, Result<string>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<User, Guid> _userRepository;
        private readonly IRepository<AccountAuth, Guid> _authRepository;
        private readonly IOtpService _otpService;
        public ResetPasswordVerifyHandler(
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            IOtpService otpService)
        {
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _otpService = otpService;

            _userRepository = _unitOfWork.GetRepository<User, Guid>();
            _authRepository = _unitOfWork.GetRepository<AccountAuth, Guid>();
        }

        public async Task<Result<string>> Handle(ResetPasswordVerifyCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedAccessException("Không xác định được người dùng.");

            var user = await _userRepository.GetByIdAsync(_currentUserService.UserId.Value)
                ?? throw new NotFoundException("Người dùng không tồn tại.");

            var account = await _authRepository
                .FirstOrDefaultAsync(a => a.UserId == user.Id && a.Provider == "Local")
                ?? throw new NotFoundException("Tài khoản chưa thiết lập phương thức đăng nhập này.");

            if (PasswordHasher.Verify(request.NewPassword, account.PasswordHash, account.Salt))
                return Result<string>.Failure("Mật khẩu mới không được trùng với mật khẩu cũ.");

            var isValidOtp = await _otpService.VerifyAsync(user.Id.ToString(), request.Otp);
            if (!isValidOtp)
                return Result<string>.Failure("OTP không hợp lệ hoặc đã hết hạn.");

            var (hash, salt) = PasswordHasher.Hash(request.NewPassword);
            account.SetPassword(hash, salt);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success("Đặt lại mật khẩu thành công.");
        }
    }
}
