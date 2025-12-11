using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.BoardMembers.Dtos
{
    public class BoardMemberDto
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public BoardRole? Role { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
