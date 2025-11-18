using MediatR;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Authentication.Commands
{
    public class LogoutCommand : IRequest<Result<string>>
    {
        public string Provider { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
    }

    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<string>>
    {
        private readonly IRepository<AccountAuth, Guid> _authRepository;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _uow;

        public LogoutCommandHandler(
            ITokenService tokenService,
            IUnitOfWork uow)
        {
            _tokenService = tokenService;
            _uow = uow;
            _authRepository = _uow.GetRepository<AccountAuth, Guid>();
        }
        public async Task<Result<string>> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var auth = await _authRepository.FirstOrDefaultAsync(a =>
                a.Provider == request.Provider &&
                a.RefreshToken == request.RefreshToken)
                ?? throw new Exception("Token không hợp lệ");

            auth.RevokeRefreshToken();
            await _authRepository.UpdateAsync(auth);
            await _uow.SaveChangesAsync(cancellationToken);

            return Result<string>.Success("Đăng xuất thành công");
        }
    }
}
