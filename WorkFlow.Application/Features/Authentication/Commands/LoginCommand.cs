using MediatR;
using Microsoft.Extensions.Configuration;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.Authentication.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Common.Helpers;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.Authentication.Commands
{
    public class LoginCommand : IRequest<Result<LoginResultDto>>
    {
        public string Type { get; set; } = default!;
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Token { get; set; }
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResultDto>>
    {
        private readonly IConfiguration _configuration;
        private readonly IRepository<AccountAuth, Guid> _authRepository;
        private readonly IRepository<User, Guid> _userRepository;
        private readonly IOAuthVerifier _oAuthVerifier;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _uow;

        public LoginCommandHandler(
            IConfiguration configuration,
            IOAuthVerifier oAuthVerifier,
            ITokenService tokenService,
            IUnitOfWork uow)
        {
            _configuration = configuration;
            _oAuthVerifier = oAuthVerifier;
            _tokenService = tokenService;
            _uow = uow;
            _authRepository = _uow.GetRepository<AccountAuth, Guid>();
            _userRepository = _uow.GetRepository<User, Guid>();
        }

        public Task<Result<LoginResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            return request.Type.ToLower() switch
            {
                "email" => HandleLocalLogin(request, cancellationToken),
                "google" => HandleGoogleLogin(request, cancellationToken),
                "facebook" => HandleFacebookLogin(request, cancellationToken),
                _ => throw new NotSupportedException($"Login type '{request.Type}' is not supported.")
            };
        }

        private async Task<Result<LoginResultDto>> HandleLocalLogin(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == request.Email)
                ?? throw new NotFoundException("Email chưa được đăng ký người dùng.");

            var auth = await _authRepository.FirstOrDefaultAsync(a => a.UserId == user.Id && a.Provider == EnumExtensions.GetName(AccountProvider.Local))
                ?? throw new AppException("Tài khoản chưa thiết lập phương thức đăng nhập này.");

            if (auth.IsLocked())
                throw new ForbiddenAccessException("Tài khoản bị khóa, vui lòng liên hệ quản trị viên theo số điện thoại 0966963030 để được hỗ trợ.");

            var isLoginValid = PasswordHasher.Verify(request.Password!, auth.PasswordHash!, auth.Salt!);
            if (!isLoginValid)
            {
                auth.MarkLoginFailed();
                await _authRepository.UpdateAsync(auth);
                await _uow.SaveChangesAsync(cancellationToken);
                throw new UnauthorizedException("Mật khẩu không đúng, vui lòng thử lại.");
            }

            auth.MarkLoginSuccess();

            var (accessToken, refreshToken) = await _tokenService.IssueAsync(user.Id, AccountProvider.Local);

            int refreshTokenExpiryDays = int.TryParse(_configuration["Jwt:RefreshTokenDays"], out int value) ? value : 7;

            auth.SetRefreshToken(refreshToken, refreshTokenExpiryDays);
            await _authRepository.UpdateAsync(auth);
            await _uow.SaveChangesAsync(cancellationToken);

            var data = new LoginResultDto
            {
                UserId = user.Id,
                Provider = auth.Provider,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            return Result<LoginResultDto>.Success(data, "Đăng nhập thành công");
        }

        private async Task<Result<LoginResultDto>> HandleGoogleLogin(LoginCommand request, CancellationToken cancellationToken)
        {
            var profile = await _oAuthVerifier.VerifyGoogleAsync(request.Token!);
            return await HandleOAuth(profile, AccountProvider.Google);
        }

        private async Task<Result<LoginResultDto>> HandleFacebookLogin(LoginCommand request, CancellationToken cancellationToken)
        {
            var profile = await _oAuthVerifier.VerifyFacebookAsync(request.Token!);
            return await HandleOAuth(profile, AccountProvider.Facebook);
        }

        private async Task<Result<LoginResultDto>> HandleOAuth(OAuthProfileDto profile, AccountProvider provider)
        {
            var auth = await _authRepository.FirstOrDefaultAsync(a => a.ProviderUid == profile.Uid && a.Provider == EnumExtensions.GetName(provider))
                ?? throw new NotFoundException("Chưa tạo tài khoản hoặc chưa kết nối phương thức đăng nhập");

            var (accessToken, refreshToken) = await _tokenService.IssueAsync(auth.UserId, provider);

            int refreshTokenExpiryDays = int.TryParse(_configuration["Jwt:RefreshTokenDays"], out int value) ? value : 7;

            auth.SetRefreshToken(refreshToken, refreshTokenExpiryDays);
            auth.MarkLoginSuccess();
            await _authRepository.UpdateAsync(auth);
            await _uow.SaveChangesAsync();

            var data = new LoginResultDto
            {
                UserId = auth.UserId,
                Provider = auth.Provider,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            return Result<LoginResultDto>.Success(data, "Đăng nhập thành công");
        }
    }
}
