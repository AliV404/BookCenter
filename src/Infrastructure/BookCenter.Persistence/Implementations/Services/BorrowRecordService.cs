using BookCenter.Application.Abstractions.Repositories;
using BookCenter.Application.Abstractions.Services;
using BookCenter.Application.Common;
using BookCenter.Application.DTOs.BorrowRecords;
using BookCenter.Application.Exceptions;
using BookCenter.Domain.Entities;
using BookCenter.Persistence.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookCenter.Persistence.Implementations.Services
{
    public class BorrowRecordService : IBorrowRecordService
    {
        private readonly IBorrowRecordRepository _borrowRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IReaderRepository _readerRepository;

        public BorrowRecordService(IBorrowRecordRepository borrowRepository, IBookRepository bookRepository, IReaderRepository readerRepository)
        {
            _borrowRepository = borrowRepository;
            _bookRepository = bookRepository;
            _readerRepository = readerRepository;
        }

        private static Expression<Func<BorrowRecord, GetBorrowRecordDto>> ToDto(DateTime now) => r => new GetBorrowRecordDto(
            r.Id,
            r.BookId, r.Book.Name,
            r.ReaderId, r.Reader.Name + " " + r.Reader.Surname,
            r.BorrowedAt, r.DueDate, r.ReturnedAt,
            r.ReturnedAt == null && r.DueDate < now);

        public async Task<PagedResult<GetBorrowRecordDto>> GetAllAsync(BorrowRecordQuery query)
        {
            var now = DateTime.UtcNow;
            var records = _borrowRepository.GetAll();

            if (query.ReaderId is not null)
                records = records.Where(r => r.ReaderId == query.ReaderId);
            if (query.BookId is not null)
                records = records.Where(r => r.BookId == query.BookId);
            if (query.Search is not null)
                records = records.Where(r => r.Book.Name.Contains(query.Search)
                                          || r.Reader.Name.Contains(query.Search)
                                          || r.Reader.Surname.Contains(query.Search));

            records = query.Status switch
            {
                BorrowStatus.Active => records.Where(r => r.ReturnedAt == null),
                BorrowStatus.Overdue => records.Where(r => r.ReturnedAt == null && r.DueDate < now),
                BorrowStatus.Returned => records.Where(r => r.ReturnedAt != null),
                _ => records
            };

            var ordered = query.Status == BorrowStatus.Overdue
                ? records.OrderBy(r => r.DueDate).ThenBy(r => r.Id)                 // most overdue first
                : records.OrderByDescending(r => r.BorrowedAt).ThenByDescending(r => r.Id); // newest first

            return await ordered.Select(ToDto(now)).ToPagedResultAsync(query);
        }

        public async Task<GetBorrowRecordDto> GetByIdAsync(int id)
        {
            return await _borrowRepository.GetAll(r => r.Id == id).Select(ToDto(DateTime.UtcNow)).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(BorrowRecord), id);
        }

        public async Task<int> BorrowBookAsync(CreateBorrowRecordDto dto)
        {
            var book = await _bookRepository.GetByIdAsync(dto.BookId)
                ?? throw new NotFoundException(nameof(Book), dto.BookId);

            if (!await _readerRepository.AnyAsync(r => r.Id == dto.ReaderId))
                throw new NotFoundException(nameof(Reader), dto.ReaderId);

            if (book.CopiesAvailable <= 0)
                throw new ConflictException($"'{book.Name}' has no copies available right now.");

            if (await _borrowRepository.AnyAsync(r => r.BookId == dto.BookId && r.ReaderId == dto.ReaderId && r.ReturnedAt == null))
                throw new ConflictException("This reader already has a copy of this book.");

            book.CopiesAvailable--;
            var record = new BorrowRecord
            {
                BookId = dto.BookId,
                ReaderId = dto.ReaderId,
                BorrowedAt = DateTime.UtcNow,
                DueDate = ToUtc(dto.DueDate)
            };
            await _borrowRepository.AddAsync(record);

            // ONE SaveChanges = one transaction: the new record and the stock decrement
            // are committed together or not at all.
            await SaveOrConflictAsync();
            return record.Id;
        }

        public async Task ReturnBookAsync(int recordId)
        {
            var record = await _borrowRepository.GetByIdAsync(recordId, nameof(BorrowRecord.Book))
                ?? throw new NotFoundException(nameof(BorrowRecord), recordId);

            if (record.ReturnedAt is not null)
                throw new ConflictException("This book has already been returned.");

            record.ReturnedAt = DateTime.UtcNow;
            record.Book.CopiesAvailable++;

            await SaveOrConflictAsync();
        }

        private async Task SaveOrConflictAsync()
        {
            try
            {
                await _borrowRepository.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Another request changed the same book between our read and our write
                // (e.g. two people borrowing the last copy). Nothing was saved.
                throw new ConflictException("The book was updated by another request at the same time. Please try again.");
            }
        }

        private static DateTime ToUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
