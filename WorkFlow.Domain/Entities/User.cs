using WorkFlow.Domain.Common;

namespace WorkFlow.Domain.Entities
{
    public class User : Entity<Guid>
    {
        public string Name { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PhoneNumber { get; private set; } = string.Empty;
    }
}
