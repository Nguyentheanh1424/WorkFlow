using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Domain.Entities
{
    public class WorkspaceMember : CreationAuditEntity<Guid>
    {
        public Guid WorkSpaceId { get; set; }
        public Guid UserId { get; set; }
        public WorkSpaceRole Role { get; set; }

        public DateTime JoinedAt { get; set; }

        protected WorkspaceMember() { }

        public static WorkspaceMember Create(Guid workspaceId, Guid userId, WorkSpaceRole role)
        {
            return new WorkspaceMember
            {
                WorkSpaceId = workspaceId,
                UserId = userId,
                Role = role,
                JoinedAt = DateTime.UtcNow
            };
        }
    }
}
