using Microsoft.EntityFrameworkCore;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Define DbSets for your entities here
        // public DbSet<YourEntity> YourEntities { get; set; }
        public DbSet<Workflow> Workflows { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AccountAuth> AccountAuth { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure your entity mappings here
        }
    }
}
