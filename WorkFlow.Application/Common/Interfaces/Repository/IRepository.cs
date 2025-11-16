using System.Linq.Expressions;
using WorkFlow.Domain.Common;

namespace WorkFlow.Application.Common.Interfaces.Repository
{
    public interface IRepository<TEntity, TId> where TEntity : Entity<TId>
    {
        IQueryable<TEntity> GetAll();
        Task<List<TEntity>> GetAllAsync();
        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity?> GetByIdAsync(TId id);
        Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
        Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null);
        Task<TId> AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TEntity entity);
        Task DeleteAsync(TId id);
    }
}
