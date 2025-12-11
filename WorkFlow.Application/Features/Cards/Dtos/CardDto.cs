using WorkFlow.Application.Common.Mappings;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.Cards.Dtos
{
    public class CardDto : IMapFrom<Card>
    {
        public Guid Id { get; set; }
        public Guid ListId { get; set; }

        public string Title { get; set; } = null!;
        public string? Background { get; set; }
        public string? Description { get; set; }

        public JobStatus Status { get; set; }
        public int[]? Label { get; set; }

        public int Position { get; set; }

        public string? PlaceId { get; set; }

        public DateTime? DueDate { get; set; }
        public DateTime? StartDate { get; set; }

        public bool ReminderEnabled { get; set; }
        public int? ReminderBeforeMinutes { get; set; }
    }
}
