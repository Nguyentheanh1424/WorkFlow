using MediatR;
using Microsoft.Extensions.Configuration;
using WorkFlow.Application.Common.Exceptions;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.Authentication.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.Authentication.Commands
{
    public class RefreshTokenCommand : IRequest<Result<TokenResultDto>>
    {
        public string RefreshToken { get; set; } = default!;
    }

    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<TokenResultDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly IRepository<AccountAuth, Guid> _authRepository;
        public RefreshTokenCommandHandler(
            IUnitOfWork uow,
            IConfiguration configuration,
            ITokenService tokenService)
        {
            _uow = uow;
            _configuration = configuration;
            _tokenService = tokenService;
            _authRepository = _uow.GetRepository<AccountAuth, Guid>();
        }
        public async Task<Result<TokenResultDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var auth = await _authRepository.FirstOrDefaultAsync(a =>
                a.RefreshToken == request.RefreshToken)
                ?? throw new UnauthorizedException("Refresh token không hợp lệ");

            if (auth.RefreshTokenExpireAt < DateTime.UtcNow)
                throw new UnauthorizedException("Refresh token đã hết hạn");

            var (newAccessToken, newRefreshToken) = await _tokenService.IssueAsync(auth.UserId, EnumExtensions.ParseEnum<AccountProvider>(auth.Provider));

            int refreshTokenExpiryDays = int.TryParse(_configuration["Jwt:RefreshTokenDays"], out int value) ? value : 7;

            auth.SetRefreshToken(newRefreshToken, refreshTokenExpiryDays);

            await _authRepository.UpdateAsync(auth);
            await _uow.SaveChangesAsync(cancellationToken);

            var data = new TokenResultDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };

            return Result<TokenResultDto>.Success(data, "Làm mới token thành công");
        }
    }
}
