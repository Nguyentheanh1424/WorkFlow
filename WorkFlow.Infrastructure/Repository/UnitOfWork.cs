using Microsoft.EntityFrameworkCore;
using System.Collections;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Domain.Common;

namespace WorkFlow.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _context;
        private Hashtable _repositories = new();

        public UnitOfWork(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy hoặc khởi tạo repository cho một entity cụ thể.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TId"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IRepository<TEntity, TId> GetRepository<TEntity, TId>()
            where TEntity : Entity<TId>
        {
            var typeName = typeof(TEntity).Name;

            if (!_repositories.ContainsKey(typeName))
            {
                var repositoryType = typeof(Repository<,>).MakeGenericType(typeof(TEntity), typeof(TId));
                var repositoryInstance = Activator.CreateInstance(repositoryType, _context);
                _repositories.Add(typeName, repositoryInstance);
            }

            return (IRepository<TEntity, TId>)_repositories[typeName]!;
        }

        /// <summary>
        /// Ghi tất cả thay đổi của context xuống database.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Giải phóng tài nguyên
        /// </summary>
        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
