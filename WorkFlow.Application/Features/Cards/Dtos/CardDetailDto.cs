using WorkFlow.Application.Common.Mappings;
using WorkFlow.Application.Features.Attachments.Dtos;
using WorkFlow.Application.Features.CardAssignees.Dtos;
using WorkFlow.Application.Features.Tasks.Dtos;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Cards.Dtos
{
    public class CardDetailDto : IMapFrom<Card>
    {
        public Guid Id { get; set; }
        public Guid ListId { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public int Position { get; set; }

        public string? Label { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public bool ReminderEnabled { get; set; }
        public int? ReminderBeforeMinutes { get; set; }

        public List<CardAssigneeDto> Assignees { get; set; } = new();
        public List<AttachmentDto> Attachments { get; set; } = new();

        public List<TaskDto> Tasks { get; set; } = new();
    }
}
