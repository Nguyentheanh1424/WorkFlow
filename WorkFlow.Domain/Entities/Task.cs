using WorkFlow.Domain.Common;

namespace WorkFlow.Domain.Entities
{
    public class Task : FullAuditEntity<Guid>
    {
        public Guid CardId { get; set; }
        public string Title { get; set; } = null!;
        public int Position { get; set; }

        protected Task() { }

        public static Task Create(Guid cardId, string title, int position)
        {
            return new Task
            {
                CardId = cardId,
                Title = title,
                Position = position
            };
        }

        public void Rename(string newTitle)
        {
            Title = newTitle;
        }

        public void MoveTo(int newPosition) => Position = newPosition;
    }
}
