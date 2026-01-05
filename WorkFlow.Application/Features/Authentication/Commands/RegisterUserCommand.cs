using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Cache;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.Authentication.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Common.Helpers;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Authentication.Commands
{
    public record RegisterUserCommand(RegisterUserDto data) : IRequest<Result>;

    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.data.Email)
                .NotEmpty().WithMessage("Email không được để trống.")
                .EmailAddress().WithMessage("Email không hợp lệ.")
                .Must(email => email.EndsWith("@gmail.com"))
                .WithMessage("Email phải thuộc domain hợp lệ.");

            RuleFor(x => x.data.Name)
                .NotEmpty().WithMessage("Tên không được để trống.")
                .MaximumLength(100).WithMessage("Tên không được vượt quá 100 ký tự.")
                .Matches(@"^[\p{L}\s]+$").WithMessage("Tên chỉ được chứa chữ cái và khoảng trắng.");

            RuleFor(x => x.data.Password)
                .NotEmpty().WithMessage("Mật khẩu không được để trống.")
                .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 ký tự.")
                .Matches("[A-Z]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ hoa (A-Z).")
                .Matches("[a-z]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ thường (a-z).")
                .Matches("[0-9]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ số (0-9).")
                .Matches("[^a-zA-Z0-9]").WithMessage("Mật khẩu phải chứa ít nhất 1 ký tự đặc biệt.");
        }
    }

    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result>
    {
        private readonly ICacheService _cacheService;
        private readonly IEmailService _emailService;
        private readonly IOtpService _otpService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<User, Guid> _userRepository;

        public RegisterUserCommandHandler(
            ICacheService cacheService,
            IEmailService emailService,
            IOtpService otpService,
            IUnitOfWork unitOfWork)
        {
            _cacheService = cacheService;
            _emailService = emailService;
            _otpService = otpService;
            _unitOfWork = unitOfWork;
            _userRepository = _unitOfWork.GetRepository<User, Guid>();
        }

        public async Task<Result> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var email = request.data.Email.Trim().ToLower();

            bool userExists = await _userRepository.AnyAsync(u => u.Email == email);
            if (userExists)
                return Result.Failure(
                    "Email này đã được sử dụng.");

            await _otpService.ValidateOtpRequestAsync(email);

            var (hash, salt) = PasswordHasher.Hash(request.data.Password);

            var pendingUser = new PendingUserCacheModel(
                email: email,
                passwordHash: hash,
                salt: salt,
                name: request.data.Name,
                ttl: TimeSpan.FromMinutes(30));

            await _cacheService.SetAsync(pendingUser);

            var otp = await _otpService.GenerateAsync(email);

            await _otpService.MarkOtpSentAsync(email);

            Console.WriteLine("OTP Code: " + otp);
            //await _emailService.SendAsync(
            //    email,
            //    "Xác thực tài khoản",
            //    $"<p>Mã OTP của bạn là: <strong>{otp}</strong></p>"
            //);

            return Result.Success("Đăng ký thành công. Vui lòng kiểm tra email để xác thực OTP.");
        }
    }
}
