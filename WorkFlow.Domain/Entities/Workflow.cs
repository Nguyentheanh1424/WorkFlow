using WorkFlow.Domain.Common;

namespace WorkFlow.Domain.Entities
{
    public class Workflow : Entity<Guid>
    {
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }
        public bool IsActive { get; private set; }

        private Workflow() { }

        public Workflow(string name, string description)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void UpdateDetails(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}
