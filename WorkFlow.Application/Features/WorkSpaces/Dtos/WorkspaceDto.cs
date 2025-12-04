using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Application.Common.Mappings;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.WorkSpaces.Dtos
{
    public class WorkspaceDto
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? Background { get; set; }
        public int Type { get; set; }
    }
}
