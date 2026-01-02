using WorkFlow.Application.Common.Mappings;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.SubTasks.Dtos
{
    public class SubTaskDto : IMapFrom<SubTask>
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public string Title { get; set; } = "";
        public int Position { get; set; }
        public JobStatus Status { get; set; }

        public DateTime? DueDate { get; set; }

        public List<Guid> Assignees { get; set; } = new();
    }
}
