using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkFlow.Domain.Common
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }
        Guid? DeletedBy { get; set; }
    }

    public abstract class FullAuditEntity<TId>
        : ModificationAuditEntity<TId>, ISoftDelete
    {
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
    }

}
