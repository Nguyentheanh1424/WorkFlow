using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Domain.Entities
{
    public class Card : FullAuditEntity<Guid>
    {
        public Guid ListId { get; set; }

        public string Title { get; set; } = null!;
        public string? Background { get; set; }
        public string? Description { get; set; }

        public JobStatus Status { get; set; } = JobStatus.Todo;
        public int[]? Label { get; set; } // List mã màu: Hardcode theo FE

        public int Position { get; set; }

        public string? PlaceId { get; set; }

        public DateTime? DueDate { get; set; }
        public DateTime? StartDate { get; set; }

        public bool ReminderEnabled { get; set; } = false;
        public int? ReminderBeforeMinutes { get; set; }

        protected Card() { }

        public static Card Create(
            Guid listId,
            string title,
            int position,
            string? background = null,
            string? description = null)
        {
            return new Card
            {
                ListId = listId,
                Title = title,
                Position = position,
                Background = background,
                Description = description,
                Status = JobStatus.Todo,
                ReminderEnabled = false
            };
        }

        public void SetStatus(JobStatus newStatus) => Status = newStatus;
        public void Rename(string newTitle) => Title = newTitle;

        public void SetDescription(string? desc) => Description = desc;

        public void SetBackground(string? background) => Background = background;

        public void MoveTo(int newPosition) => Position = newPosition;

        public void MoveToList(Guid newListId)
        {
            ListId = newListId;
        }

        public void SetDueDate(DateTime? due) => DueDate = due;

        public void SetStartDate(DateTime? start) => StartDate = start;

        public void EnableReminder(int beforeMinutes)
        {
            ReminderEnabled = true;
            ReminderBeforeMinutes = beforeMinutes;
        }

        public void DisableReminder()
        {
            ReminderEnabled = false;
            ReminderBeforeMinutes = null;
        }
    }
}
