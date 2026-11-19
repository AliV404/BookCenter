using BookCenter.Domain.Base;
using System.Linq.Expressions;

namespace BookCenter.Application.Abstractions.Repositories.Generic
{
    public interface IRepository<T> where T : BaseEntity
    {
        /// <summary>
        /// Returns a composable query. Soft-deleted rows are already excluded by a global query filter.
        /// Ordering, paging and projection are left to the caller.
        /// </summary>
        IQueryable<T> GetAll(Expression<Func<T, bool>>? expression = null, bool isTracking = false, params string[] includes);

        /// <summary>Loads a tracked entity (ready to be modified) or null when it doesn't exist.</summary>
        Task<T?> GetByIdAsync(int id, params string[] includes);

        Task<bool> AnyAsync(Expression<Func<T, bool>> expression);

        Task AddAsync(T entity);

        /// <summary>Soft delete: the row is kept and flagged IsDeleted (see AppDbContext.SaveChangesAsync).</summary>
        void Delete(T entity);

        Task<int> SaveChangesAsync();
    }
}
