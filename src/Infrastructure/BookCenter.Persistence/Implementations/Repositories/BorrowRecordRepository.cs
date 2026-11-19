using BookCenter.Application.Abstractions.Repositories;
using BookCenter.Domain.Entities;
using BookCenter.Persistence.Data;
using BookCenter.Persistence.Implementations.Repositories.Generic;

namespace BookCenter.Persistence.Implementations.Repositories
{
    public class BorrowRecordRepository : Repository<BorrowRecord>, IBorrowRecordRepository
    {
        public BorrowRecordRepository(AppDbContext context) : base(context) { }
    }
}
