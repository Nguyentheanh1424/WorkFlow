using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Domain.Entities
{
    public class BoardMember : CreationAuditEntity<Guid>
    {
        public Guid BoardId { get; set; }
        public Guid UserId { get; set; }

        public BoardRole? Role { get; set; }

        public DateTime JoinedAt { get; set; }

        protected BoardMember() { }

        public static BoardMember Create(Guid boardId, Guid userId, BoardRole role)
        {
            return new BoardMember
            {
                BoardId = boardId,
                UserId = userId,
                Role = role,
                JoinedAt = DateTime.UtcNow
            };
        }
    }
}
