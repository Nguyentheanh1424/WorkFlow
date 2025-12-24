using WorkFlow.Application.Common.Mappings;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Comments.Dtos
{
    public class CommentDto : IMapFrom<Comment>
    {
        public Guid Id { get; set; }
        public Guid CardId { get; set; }
        public Guid UserId { get; set; }

        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
