using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.Boards.Dtos
{
    public class CreateBoardDto
    {
        public Guid WorkspaceId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Background { get; set; }
        public int[]? Label { get; set; }
        public VisibilityBoard Visibility { get; set; }
    }
}
