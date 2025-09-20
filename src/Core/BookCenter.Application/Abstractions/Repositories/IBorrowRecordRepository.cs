using BookCenter.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.Abstractions.Repositories
{
    public interface IBorrowRecordRepository
    {
        Task<BorrowRecord?> GetAsync(int bookId, int readerId);
        Task AddAsync(BorrowRecord record);
        void Update(BorrowRecord record);
        Task<IEnumerable<BorrowRecord>> GetBorrowedByReaderAsync(int readerId);
        Task<IEnumerable<BorrowRecord>> GetOverdueRecordsAsync();
        Task<int> SaveChangesAsync();
    }
}
