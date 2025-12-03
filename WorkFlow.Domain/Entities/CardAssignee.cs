using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Domain.Common;

namespace WorkFlow.Domain.Entities
{
    public class CardAssignee : CreationAuditEntity<Guid>
    {
        public Guid CardId { get; set; }
        public Guid UserId { get; set; }

        protected CardAssignee() { }

        public static CardAssignee Create(Guid cardId, Guid userId)
        {
            return new CardAssignee
            {
                CardId = cardId,
                UserId = userId
            };
        }
    }
}
