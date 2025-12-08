using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkFlow.Application.Features.Tasks.Dtos
{
    public class SubTaskDto
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public string Title { get; set; } = "";
        public bool IsDone { get; set; }

        public List<Guid> Assignees { get; set; } = new();
    }
}
