using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Domain.Entities
{
    public class WorkSpace : FullAuditEntity<Guid>
    {
        public string Name { get; set; } = null!;
        public string? Background { get; set; }
        public string? Description { get; set; }
        public WorkSpaceType Type { get; set; }

        protected WorkSpace() { }

        public static WorkSpace Create(string name, WorkSpaceType type, string? background, string? description)
        {
            return new WorkSpace
            {
                Name = name,
                Type = type,
                Background = background,
                Description = description
            };
        }

        public void Rename(string newName)
        {
            Name = newName;
        }

        public void ChangeDescription(string? newDescription)
        {
            Description = newDescription;
        }

        public void ChangeBackground(string? newBackground)
        {
            Background = newBackground;
        }
    }
}
