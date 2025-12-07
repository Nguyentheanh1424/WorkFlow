using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Application.Common.Mappings;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.Boards.Dtos
{
    public class BoardDto : IMapFrom<Board>
    {
        public Guid WorkspaceId { get; set; }
        public Guid OwnerId { get; set; }

        public string Title { get; set; } = null!;
        public VisibilityBoard Visibility { get; set; }

        public string? Background { get; set; }
        public string? Description { get; set; }

        public bool Pinned { get; set; }
    }
}
