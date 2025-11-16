using MediatR;
using WorkFlow.Application.Features.Workflows.Dtos;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Workflows.Commands
{
    public record LoginUserCommand(LoginUserDto Dto) : IRequest<string>;

    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, string>
    {
        private readonly IUnitOfWork _unit;
        private readonly IPasswordHasher _hasher;

        public LoginUserCommandHandler(IUnitOfWork unit, IPasswordHasher hasher)
        {
            _unit = unit;
            _hasher = hasher;
        }

        public async Task<string> Handle(LoginUserCommand request, CancellationToken ct)
        {
            var repo = _unit.GetRepository<AccountAuth<Guid>, Guid>();
            var account = (await repo.FindAsync(x => x.ProviderUid == request.Dto.Email)).FirstOrDefault()
                ?? throw new Exception("Email không tồn tại.");

            // Verify BCrypt
            if (!_hasher.Verify(request.Dto.Password, account.HashPassword))
                throw new Exception("Mật khẩu sai.");

            account.UpdateLastLogin();
            await repo.UpdateAsync(account);
            await _unit.SaveChangesAsync(ct);

            var token = Guid.NewGuid().ToString();
            account.SetAccessToken(token);
            return token;
        }
    }
}
