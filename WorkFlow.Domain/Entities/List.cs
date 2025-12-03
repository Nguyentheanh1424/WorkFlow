using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Domain.Common;

namespace WorkFlow.Domain.Entities
{
    public class List : FullAuditEntity<Guid>
    {
        public Guid BoardId { get; set; }

        public string Title { get; set; } = null!;
        public int Position { get; set; }
        public bool IsArchived { get; set; }

        protected List() { }

        public static List Create(Guid boardId, string title, int position)
        {
            return new List
            {
                BoardId = boardId,
                Title = title,
                Position = position,
                IsArchived = false
            };
        }

        public void Rename(string newTitle) => Title = newTitle;

        public void MoveTo(int newPosition) => Position = newPosition;

        public void MoveToBoard(Guid newBoardId) => BoardId = newBoardId;

        public void Archive() => IsArchived = true;

        public void Unarchive() => IsArchived = false;
    }
}
