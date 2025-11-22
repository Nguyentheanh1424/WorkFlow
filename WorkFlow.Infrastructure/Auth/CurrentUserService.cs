using Microsoft.AspNetCore.Http;
using WorkFlow.Application.Common.Interfaces.Auth;

namespace WorkFlow.Infrastructure.Auth
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst("userId")?
                    .Value;

                return Guid.TryParse(userIdClaim, out var id) ? id : null;
            }
        }

        public string? Provider =>
            _httpContextAccessor.HttpContext?
                .User?
                .FindFirst("provider")?
                .Value;
    }
}
