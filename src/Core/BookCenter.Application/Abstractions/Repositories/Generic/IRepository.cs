using BookCenter.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.Abstractions.Repositories.Generic
{
    public interface IRepository<T> where T : BaseEntity, new()
    {
        IQueryable<T> GetAll
            (
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>? orderExpression = null,
            int skip = 0,
            int take = 0,
            bool isTracking = false,
            bool isDescending = false,
            params string[] includes
            );

        Task<T> GetByIdAsync(int id, params string[] includes);

        Task AddAsync(T entity);

        void Update(T entity);

        void Delete(T entity);
        Task<bool> AnyAsync(Expression<Func<T, bool>> expression);
        Task<int> SaveChangesAsync();
    }
}
