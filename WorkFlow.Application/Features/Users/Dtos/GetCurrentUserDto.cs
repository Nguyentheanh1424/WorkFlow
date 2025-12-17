using WorkFlow.Application.Common.Mappings;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Users.Dtos
{
    public class GetCurrentUserDto : IMapFrom<User>
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        public string AvatarUrl { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }

        public bool IsGoogleLinked { get; set; }

        public bool IsFacebookLinked { get; set; }
    }
}
