using BookCenter.Application.Abstractions.Repositories.Generic;
using BookCenter.Domain.Base;
using BookCenter.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookCenter.Persistence.Implementations.Repositories.Generic
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly AppDbContext _context;
        private readonly DbSet<T> _table;

        public Repository(AppDbContext context)
        {
            _context = context;
            _table = context.Set<T>();
        }

        public IQueryable<T> GetAll(Expression<Func<T, bool>>? expression = null, bool isTracking = false, params string[] includes)
        {
            IQueryable<T> query = _table;

            if (expression != null)
                query = query.Where(expression);

            foreach (var include in includes)
                query = query.Include(include);

            return isTracking ? query : query.AsNoTracking();
        }

        public async Task<T?> GetByIdAsync(int id, params string[] includes)
        {
            IQueryable<T> query = _table;

            foreach (var include in includes)
                query = query.Include(include);

            return await query.FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<bool> AnyAsync(Expression<Func<T, bool>> expression) => _table.AnyAsync(expression);

        public async Task AddAsync(T entity) => await _table.AddAsync(entity);

        public void Delete(T entity) => _table.Remove(entity);

        // All repositories share the same scoped AppDbContext, so this saves every pending
        // change made through any repository in the current request, in one transaction.
        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
