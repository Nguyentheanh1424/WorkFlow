using WorkFlow.Application.Common.Mappings;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Workflows.Dtos
{
    public class WorkflowDto : IMapFrom<Workflow>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
