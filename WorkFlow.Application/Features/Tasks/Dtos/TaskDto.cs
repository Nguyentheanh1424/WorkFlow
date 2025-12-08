using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Application.Common.Mappings;

namespace WorkFlow.Application.Features.Tasks.Dtos
{
    public class TaskDto : IMapFrom<Task>
    {
        public Guid Id { get; set; }
        public Guid CardId { get; set; }
        public string Title { get; set; } = "";

        public List<SubTaskDto> SubTasks { get; set; } = new();
    }
}
