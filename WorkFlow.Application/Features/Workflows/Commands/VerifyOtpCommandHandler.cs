using MediatR;
using WorkFlow.Application.Features.Workflows.Dtos;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Common.Cache;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Workflows.Commands
{
    public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, bool>
    {
        private readonly ICacheService _cache;
        private readonly IUnitOfWork _unit;
        private readonly IPasswordHasher _hasher;

        public VerifyOtpCommandHandler(ICacheService cache, IUnitOfWork unit, IPasswordHasher hasher)
        {
            _cache = cache;
            _unit = unit;
            _hasher = hasher;
        }

        public async Task<bool> Handle(VerifyOtpCommand request, CancellationToken ct)
        {
            var cacheKey = $"PENDING_USER:{request.Dto.Email}";
            Console.WriteLine($"[DEBUG] VERIFY using cache key: {cacheKey}");

            var pending = await _cache.GetAsync<PendingUserCacheModel>(cacheKey);

            if (pending == null)
            {
                Console.WriteLine("[DEBUG] Cache MISS!");
                throw new Exception("OTP không tồn tại hoặc đã hết hạn.");
            }
            Console.WriteLine($"[DEBUG] Cached OTP: {pending.Otp}");
            Console.WriteLine($"[DEBUG] Client OTP: {request.Dto.Code}");
            if (pending.Otp != request.Dto.Code)
                return false;

            // Create User
            var userId = Guid.NewGuid();
            var user = (User)Activator.CreateInstance(typeof(User), nonPublic: true)!;
            typeof(User).GetProperty("Id")!.SetValue(user, userId);
            typeof(User).GetProperty("Name")!.SetValue(user, pending.Name);
            typeof(User).GetProperty("Email")!.SetValue(user, pending.Email);
            var userRepo = _unit.GetRepository<User, Guid>();
            await userRepo.AddAsync(user);
            await _unit.SaveChangesAsync(ct);

            // Create AccountAuth
            var authRepo = _unit.GetRepository<AccountAuth<Guid>, Guid>();
            var account = new AccountAuth<Guid>(userId);
            account.SetProviderUid(pending.Email);
            account.SetPassword(_hasher.Hash(pending.PlainPassword)); 
            await authRepo.AddAsync(account);
            await _unit.SaveChangesAsync(ct);
            // Delete cache
            await _cache.RemoveAsync(cacheKey);

            return true;
        }
    }
}
