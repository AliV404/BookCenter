using BookCenter.Application.Abstractions.Repositories;
using BookCenter.Domain.Entities;
using BookCenter.Persistance.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Persistance.Implementations.Repositories
{
    public class BorrowRecordRepository : IBorrowRecordRepository
    {
        private readonly AppDbContext _context;

        public BorrowRecordRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<BorrowRecord?> GetAsync(int bookId, int readerId)
        {
            return await _context.BorrowRecords
                .Include(r => r.Book)
                .Include(r => r.Reader)
                .FirstOrDefaultAsync(r => r.BookId == bookId && r.ReaderId == readerId && r.ReturnedAt == null);
        }

        public async Task AddAsync(BorrowRecord record)
        {
            await _context.BorrowRecords.AddAsync(record);
        }
        public void Update(BorrowRecord record)
        {
             _context.BorrowRecords.Update(record);
        }

        public async Task<IEnumerable<BorrowRecord>> GetBorrowedByReaderAsync(int readerId)
        {
            return await _context.BorrowRecords
                .Include(r => r.Book)
                .Include(r => r.Reader)
                .Where(r=> r.ReaderId==readerId && r.ReturnedAt == null)
                .ToListAsync();
        }

        public async Task<IEnumerable<BorrowRecord>> GetOverdueRecordsAsync()
        {
            return await _context.BorrowRecords
                .Include(r=> r.Book)
                .Include(r=> r.Reader)
                .Where(r=> r.DueDate < DateTime.UtcNow && r.ReturnedAt == null)
                .ToListAsync();
        }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
