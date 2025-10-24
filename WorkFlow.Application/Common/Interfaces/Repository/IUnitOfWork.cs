using WorkFlow.Domain.Common;

namespace WorkFlow.Application.Common.Interfaces.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<TEntity, TId> GetRepository<TEntity, TId>()
            where TEntity : Entity<TId>;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
