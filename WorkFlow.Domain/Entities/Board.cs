using WorkFlow.Domain.Common;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Domain.Entities
{
    public class Board : FullAuditEntity<Guid>
    {
        public Guid WorkSpaceId { get; set; }
        public Guid OwnerId { get; set; }

        public string Title { get; set; } = null!;
        public VisibilityBoard Visibility { get; set; }

        public string? Background { get; set; }
        public string? Description { get; set; }

        public bool Pinned { get; set; }
        public bool IsArchived { get; set; }

        protected Board() { }

        public static Board Create(Guid workspaceId, Guid ownerId, string title, VisibilityBoard visibility,
                                   string? background = null, string? description = null)
        {
            return new Board
            {
                WorkSpaceId = workspaceId,
                OwnerId = ownerId,
                Title = title,
                Visibility = visibility,
                Background = background,
                Description = description,
                Pinned = false,
                IsArchived = false
            };
        }

        public void Rename(string newTitle)
        {
            Title = newTitle;
        }

        public void ChangeVisibility(VisibilityBoard newVisibility)
        {
            Visibility = newVisibility;
        }

        public void SetBackground(string? background)
        {
            Background = background;
        }

        public void SetDescription(string? description)
        {
            Description = description;
        }

        public void Pin() => Pinned = true;

        public void Unpin() => Pinned = false;

        public void Archive() => IsArchived = true;
        public void Restore() => IsArchived = false;
    }
}
