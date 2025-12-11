using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.WorkSpaces.Dtos
{
    public class CreateWorkspaceDto()
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Background { get; set; }
        public WorkSpaceType Type { get; set; }
    }
}
