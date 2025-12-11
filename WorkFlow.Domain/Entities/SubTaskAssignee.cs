using WorkFlow.Domain.Common;

namespace WorkFlow.Domain.Entities
{
    public class SubTaskAssignee : CreationAuditEntity<Guid>
    {
        public Guid SubTaskId { get; set; }
        public Guid UserId { get; set; }

        protected SubTaskAssignee() { }

        public static SubTaskAssignee Create(Guid subTaskId, Guid userId)
        {
            return new SubTaskAssignee
            {
                SubTaskId = subTaskId,
                UserId = userId
            };
        }
    }
}
