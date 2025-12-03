using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Domain.Entities
{
    public class SubTask : FullAuditEntity<Guid>
    {
        public Guid TaskId { get; set; }

        public string Title { get; set; } = null!;
        public JobStatus Status { get; set; } = JobStatus.Todo;

        public DateTime? DueDate { get; set; }

        public bool ReminderEnabled { get; set; } = false;
        public int? ReminderBeforeMinutes { get; set; }

        protected SubTask() { }

        public static SubTask Create(Guid taskId, string title)
        {
            return new SubTask
            {
                TaskId = taskId,
                Title = title,
                Status = JobStatus.Todo,
                ReminderEnabled = false
            };
        }

        public void Rename(string newTitle) => Title = newTitle;

        public void UpdateStatus(JobStatus newStatus) => Status = newStatus;

        public void SetDueDate(DateTime? due) => DueDate = due;

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
