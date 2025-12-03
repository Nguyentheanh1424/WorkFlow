using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkFlow.Domain.Common
{
    public interface IHasModificationTime
    {
        DateTime? UpdatedAt { get; set; }
        Guid? UpdatedBy { get; set; }
    }

    public abstract class ModificationAuditEntity<TId>
        : CreationAuditEntity<TId>, IHasModificationTime
    {
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
    }

}
