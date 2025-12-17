using MediatR;
using Microsoft.Extensions.Configuration;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Repositories;
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
        public string Type { get; set; } = AccountProvider.Local.ToString();
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
            var type = request.Type?.Trim().ToLowerInvariant();

            return type switch
            {
                "local" => HandleLocalLogin(request, cancellationToken),
                "google" => HandleOAuthLogin(request, AccountProvider.Google, cancellationToken),
                "facebook" => HandleOAuthLogin(request, AccountProvider.Facebook, cancellationToken),
                _ => throw new NotSupportedException("Chỉ hỗ trợ local, google hoặc facebook.")
            };
        }

        private async Task<Result<LoginResultDto>> HandleLocalLogin(LoginCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new AppException("Email không được để trống.");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new AppException("Mật khẩu không được để trống.");

            var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == request.Email)
                ?? throw new NotFoundException("Email chưa được đăng ký người dùng.");

            var providerName = EnumExtensions.GetName(AccountProvider.Local);

            var auth = await _authRepository.FirstOrDefaultAsync(a => a.UserId == user.Id && a.Provider == providerName)
                ?? throw new AppException("Tài khoản chưa thiết lập phương thức đăng nhập này.");

            var (isUsable, message) = auth.IsActive();
            if (!isUsable)
                throw new ForbiddenAccessException(message);

            var (isLocked, remaining) = auth.IsLocked();
            if (isLocked)
                throw new ForbiddenAccessException($"Tài khoản bị khóa trong {remaining} phút.");

            var isLoginValid = PasswordHasher.Verify(request.Password, auth.PasswordHash, auth.Salt);
            if (!isLoginValid)
            {
                var lockMessage = auth.MarkLoginFailed();
                await _authRepository.UpdateAsync(auth);
                await _uow.SaveChangesAsync(cancellationToken);

                throw new UnauthorizedException($"Mật khẩu không đúng. {lockMessage}");
            }

            auth.MarkLoginSuccess();

            var (accessToken, refreshToken) = await _tokenService.IssueAsync(user.Id, AccountProvider.Local);

            int refreshTokenExpiryDays = GetRefreshTokenExpiryDays();
            auth.SetRefreshToken(refreshToken, refreshTokenExpiryDays);

            await _authRepository.UpdateAsync(auth);
            await _uow.SaveChangesAsync(cancellationToken);

            return Result<LoginResultDto>.Success(new LoginResultDto
            {
                UserId = user.Id,
                Provider = auth.Provider,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            }, "Đăng nhập thành công");
        }

        private async Task<Result<LoginResultDto>> HandleOAuthLogin(
            LoginCommand request,
            AccountProvider provider,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
                throw new AppException("Token không được để trống.");

            OAuthProfileDto profile = provider switch
            {
                AccountProvider.Google => await _oAuthVerifier.VerifyGoogleAsync(request.Token),
                AccountProvider.Facebook => await _oAuthVerifier.VerifyFacebookAsync(request.Token),
                _ => throw new NotSupportedException()
            };

            return await HandleOAuth(profile, provider, cancellationToken);
        }

        private async Task<Result<LoginResultDto>> HandleOAuth(
            OAuthProfileDto profile,
            AccountProvider provider,
            CancellationToken cancellationToken)
        {
            var providerName = EnumExtensions.GetName(provider);

            var auth = await _authRepository.FirstOrDefaultAsync(a =>
                a.Provider == providerName &&
                a.ProviderUid == profile.Uid);

            if (auth == null)
                throw new UnauthorizedException("Thông tin đăng nhập không hợp lệ.");

            var (isUsable, message) = auth.IsActive();
            if (!isUsable)
                throw new ForbiddenAccessException(message);

            var (isLocked, remaining) = auth.IsLocked();
            if (isLocked)
                throw new ForbiddenAccessException($"Tài khoản bị khóa trong {remaining} phút.");

            auth.MarkLoginSuccess();

            var (accessToken, refreshToken) = await _tokenService.IssueAsync(auth.UserId, provider);

            int refreshTokenExpiryDays = GetRefreshTokenExpiryDays();
            auth.SetRefreshToken(refreshToken, refreshTokenExpiryDays);

            await _authRepository.UpdateAsync(auth);
            await _uow.SaveChangesAsync(cancellationToken);

            return Result<LoginResultDto>.Success(new LoginResultDto
            {
                UserId = auth.UserId,
                Provider = auth.Provider,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            }, "Đăng nhập thành công");
        }

        private int GetRefreshTokenExpiryDays()
        {
            return int.TryParse(_configuration["Jwt:RefreshTokenDays"], out int value) ? value : 7;
        }
    }
}
