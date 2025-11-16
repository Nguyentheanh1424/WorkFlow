using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Cache;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.Authentication.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Authentication.Commands
{
    public record class VerifyRegisterOtpCommand(
        VerifyRegisterOtpCommandDto data) : IRequest<Result<string>>;

    public class VerifyRegisterOtpCommandValidator : AbstractValidator<VerifyRegisterOtpCommand>
    {
        public VerifyRegisterOtpCommandValidator()
        {
            RuleFor(x => x.data.Email)
                .NotEmpty().WithMessage("Email không được để trống.")
                .EmailAddress().WithMessage("Email không hợp lệ.")
                .Must(email => email.EndsWith("@gmail.com"))
                .WithMessage("Email phải thuộc domain hợp lệ.");

            RuleFor(x => x.data.Otp)
                .NotEmpty().WithMessage("Không có mã OTP");
        }
    }

    public class VerifyRegisterOtpCommandHandler : IRequestHandler<VerifyRegisterOtpCommand, Result<string>>
    {
        private readonly IOtpService _otpService;
        private readonly ICacheService _cacheService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<User, Guid> _userRepository;
        private readonly IRepository<AccountAuth, Guid> _accountAuthRepository;

        public VerifyRegisterOtpCommandHandler(
            IOtpService otpService,
            ICacheService cacheService,
            IUnitOfWork unitOfWork)
        {
            _otpService = otpService;
            _cacheService = cacheService;
            _unitOfWork = unitOfWork;
            _userRepository = _unitOfWork.GetRepository<User, Guid>();
            _accountAuthRepository = _unitOfWork.GetRepository<AccountAuth, Guid>();
        }

        public async Task<Result<string>> Handle(VerifyRegisterOtpCommand request, CancellationToken cancellationToken)
        {
            var email = request.data.Email.Trim().ToLower();

            var key = email;
            var isValidOtp = await _otpService.VerifyAsync(key, request.data.Otp);

            if (!isValidOtp)
                return Result<string>.Failure("Mã OTP không đúng hoặc đã hết hạn.");

            var cacheKey = $"pending-user:{email}";
            var pendingUser = await _cacheService.GetAsync<PendingUserCacheModel>(cacheKey);

            if (pendingUser == null)
                return Result<string>.Failure(
                    "Thông tin đăng ký không tồn tại hoặc đã hết hạn. Vui lòng đăng ký lại.");

            var user = new User(
                pendingUser.Name,
                pendingUser.Email
            );

            var userId = await _userRepository.AddAsync(user);

            var auth = new AccountAuth(
                userId: userId,
                passwordHash: pendingUser.PasswordHash,
                salt: pendingUser.Salt
            );

            await _accountAuthRepository.AddAsync(auth);

            await _unitOfWork.SaveChangesAsync();

            await _cacheService.RemoveAsync(cacheKey);

            return Result<string>.Success("Xác thực OTP thành công. Tài khoản đã được tạo.");
        }
    }
}
