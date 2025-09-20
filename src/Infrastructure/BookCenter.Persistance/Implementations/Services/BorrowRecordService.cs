using AutoMapper;
using BookCenter.Application.Abstractions.Repositories;
using BookCenter.Application.Abstractions.Services;
using BookCenter.Application.DTOs.BorrowRecords;
using BookCenter.Domain.Entities;
using BookCenter.Persistance.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Persistance.Implementations.Services
{
    public class BorrowRecordService : IBorrowRecordService
    {
        private readonly AppDbContext _context;
        private readonly IBorrowRecordRepository _borrowRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;

        public BorrowRecordService(IBorrowRecordRepository borrowRepository, IBookRepository bookRepository, IMapper mapper)
        {
            _borrowRepository = borrowRepository;
            _bookRepository = bookRepository;
            _mapper = mapper;
        }
        public async Task BorrowBookAsync(CreateBorrowRecordDto recordDto)
        {
            var book = await _bookRepository.GetByIdAsync(recordDto.BookId);
            if (book == null || book.CopiesAvailable <= 0)
                throw new Exception("Book is not available.");

            var record = _mapper.Map<BorrowRecord>(recordDto);
            
            await _borrowRepository.AddAsync(record);
            await _borrowRepository.SaveChangesAsync();

            book.CopiesAvailable--;
            _bookRepository.Update(book);
            await _bookRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<GetBorrowRecordDto>> GetBorrowedBooksAsync(int readerId)
        {
            var records = await _borrowRepository.GetBorrowedByReaderAsync(readerId);
            return _mapper.Map<IEnumerable<GetBorrowRecordDto>>(records);
        }

        public async Task<IEnumerable<GetBorrowRecordDto>> GetOverdueBooksAsync()
        {
            var records = await _borrowRepository.GetOverdueRecordsAsync();
            return _mapper.Map<IEnumerable<GetBorrowRecordDto>>(records);
        }

        public async Task ReturnBookAsync(int readerId, int bookId)
        {
            var record = await _borrowRepository.GetAsync(bookId, readerId);

            if (record == null)
                throw new Exception("Borrow record not found or already returned.");

            record.ReturnedAt = DateTime.Now;

            _borrowRepository.Update(record);
            await _borrowRepository.SaveChangesAsync();

            var book = await _bookRepository.GetByIdAsync(bookId);

            if(book != null)
            {
                book.CopiesAvailable++;
                _bookRepository.Update(book);
                await _bookRepository.SaveChangesAsync();
            }
        }
    }
}
