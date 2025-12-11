using WorkFlow.Domain.Common;

namespace WorkFlow.Domain.Entities
{
    public class Comment : ModificationAuditEntity<Guid>
    {
        public Guid CardId { get; set; }
        public Guid UserId { get; set; }

        public string Content { get; set; } = null!;

        protected Comment() { }

        public static Comment Create(Guid cardId, Guid userId, string content)
        {
            return new Comment
            {
                CardId = cardId,
                UserId = userId,
                Content = content
            };
        }

        public void Edit(string newContent)
        {
            Content = newContent;
        }
    }
}
