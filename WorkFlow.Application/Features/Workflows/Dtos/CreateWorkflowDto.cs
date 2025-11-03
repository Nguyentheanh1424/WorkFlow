using WorkFlow.Application.Common.Mappings;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Workflows.Dtos
{
    public class CreateWorkflowDto : IMapFrom<Workflow>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
