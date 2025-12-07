using WorkFlow.Domain.Enums;

namespace WorkFlow.API.Controllers
{
    public class UpdateNameRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateDescriptionRequest
    {
        public string? Description { get; set; }
    }

    public class UpdateBackgroundRequest
    {
        public string? Background { get; set; }
    }

    public class UpdateTypeRequest
    {
        public WorkSpaceType Type { get; set; }
    }
}
