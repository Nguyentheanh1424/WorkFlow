using Microsoft.EntityFrameworkCore;
using WorkFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;


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
        public DbSet<AccountAuth<Guid>> AccountAuth { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure your entity mappings here
            // sửa datetime thành utc
             var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v.ToUniversalTime(), 
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc) 
            );
            var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? v.Value.ToUniversalTime() : null,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null
            );
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(DateTime));
                foreach (var property in properties)
                {
                    modelBuilder.Entity(entityType.Name).Property(property.Name)
                        .HasConversion(dateTimeConverter);
                }

                var nullableProperties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(DateTime?));
                foreach (var property in nullableProperties)
                {
                    modelBuilder.Entity(entityType.Name).Property(property.Name)
                        .HasConversion(nullableDateTimeConverter);
                }
            }
        }
    }
}
