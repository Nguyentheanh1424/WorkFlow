using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.WorkSpaceMembers.Dtos
{
    public class WorkSpaceMemberDto
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public WorkSpaceRole Role { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
