using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkFlow.Domain.Common
{
    public abstract class AuditableEntity<TId> : Entity<TId>, IAuditable
    {
        private DateTime _createdAt = DateTime.UtcNow;
        [Column("CreatedAt")]
        public DateTime CreatedAt
        {
            get => _createdAt;
            set => _createdAt = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        private DateTime _updatedAt = DateTime.UtcNow;
        [Column("UpdatedAt")]
        public DateTime UpdatedAt
        {
            get => _updatedAt;
            set => _updatedAt = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
    }
}
