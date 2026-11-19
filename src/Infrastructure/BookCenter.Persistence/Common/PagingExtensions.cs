using BookCenter.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace BookCenter.Persistence.Common
{
    public static class PagingExtensions
    {
        /// <summary>
        /// Runs COUNT + one page of the (already ordered and projected) query.
        /// Always order before calling this, otherwise SQL Server may return pages in a random order.
        /// </summary>
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, PaginationQuery pagination)
        {
            var total = await query.CountAsync();
            var items = await query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            return new PagedResult<T>(items, total, pagination.Page, pagination.PageSize);
        }
    }
}
