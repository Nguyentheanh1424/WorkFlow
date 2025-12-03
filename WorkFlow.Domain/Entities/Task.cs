using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Domain.Common;

namespace WorkFlow.Domain.Entities
{
    public class Task : FullAuditEntity<Guid>
    {
        public Guid CardId { get; set; }
        public string Title { get; set; } = null!;

        protected Task() { }

        public static Task Create(Guid cardId, string title)
        {
            return new Task
            {
                CardId = cardId,
                Title = title
            };
        }

        public void Rename(string newTitle)
        {
            Title = newTitle;
        }
    }
}
