using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WorkFlow.Domain.Common
{
    public interface IHasCreationTime
    {
        DateTime CreatedAt { get; set; }
        Guid? CreatedBy { get; set; }
    }

    public abstract class CreationAuditEntity<TId> : Entity<TId>, IHasCreationTime
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
    }

}
