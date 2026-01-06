using WorkFlow.Application.Common.Mappings;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.WorkSpaces.Dtos
{
    public class WorkSpaceDetailDto : IMapFrom<WorkSpace>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string? Background { get; set; }
        public WorkSpaceType Type { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string Role { get; set; } = "";

        public int TotalMembers { get; set; }
        public int TotalBoards { get; set; }
    }

}
