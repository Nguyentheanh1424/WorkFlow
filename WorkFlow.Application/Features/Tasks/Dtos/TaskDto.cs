using WorkFlow.Application.Common.Mappings;
using WorkFlow.Application.Features.SubTasks.Dtos;

namespace WorkFlow.Application.Features.Tasks.Dtos
{
    public class TaskDto : IMapFrom<WorkFlow.Domain.Entities.Task>
    {
        public Guid Id { get; set; }
        public Guid CardId { get; set; }
        public string Title { get; set; } = "";
        public int Position { get; set; }

        public List<SubTaskDto> SubTasks { get; set; } = new();

        public int TotalSubTasks { get; set; }
        public int CompletedSubTasks { get; set; }
        public double Progress { get; set; }
    }
}
