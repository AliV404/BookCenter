using BookCenter.Application.Abstractions.Repositories;
using BookCenter.Application.Abstractions.Services;
using BookCenter.Application.Common;
using BookCenter.Application.DTOs.Readers;
using BookCenter.Application.Exceptions;
using BookCenter.Domain.Entities;
using BookCenter.Persistence.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookCenter.Persistence.Implementations.Services
{
    public class ReaderService : IReaderService
    {
        private readonly IReaderRepository _readerRepository;
        private readonly IBorrowRecordRepository _borrowRecordRepository;

        private static readonly Expression<Func<Reader, GetReaderDto>> ToDto = r => new GetReaderDto(
            r.Id, r.Name, r.Surname, r.Email, r.Phone,
            r.BorrowRecords.Count(br => br.ReturnedAt == null));

        public ReaderService(IReaderRepository readerRepository, IBorrowRecordRepository borrowRecordRepository)
        {
            _readerRepository = readerRepository;
            _borrowRecordRepository = borrowRecordRepository;
        }

        public async Task<PagedResult<GetReaderDto>> GetAllAsync(PaginationQuery query)
        {
            var readers = _readerRepository.GetAll();
            if (query.Search is not null)
                readers = readers.Where(r => r.Name.Contains(query.Search)
                                          || r.Surname.Contains(query.Search)
                                          || r.Email.Contains(query.Search));

            return await readers
                .OrderBy(r => r.Surname).ThenBy(r => r.Name).ThenBy(r => r.Id)
                .Select(ToDto)
                .ToPagedResultAsync(query);
        }

        public async Task<GetReaderDto> GetByIdAsync(int id)
        {
            return await _readerRepository.GetAll(r => r.Id == id).Select(ToDto).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Reader), id);
        }

        public async Task<int> CreateAsync(CreateReaderDto dto)
        {
            var email = NormalizeEmail(dto.Email);
            await EnsureEmailIsFreeAsync(email);

            var reader = new Reader { Name = dto.Name, Surname = dto.Surname, Email = email, Phone = dto.Phone.Trim() };
            await _readerRepository.AddAsync(reader);
            await _readerRepository.SaveChangesAsync();
            return reader.Id;
        }

        public async Task UpdateAsync(int id, UpdateReaderDto dto)
        {
            var reader = await _readerRepository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Reader), id);

            var email = NormalizeEmail(dto.Email);
            await EnsureEmailIsFreeAsync(email, exceptId: id);

            reader.Name = dto.Name;
            reader.Surname = dto.Surname;
            reader.Email = email;
            reader.Phone = dto.Phone.Trim();
            await _readerRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var reader = await _readerRepository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Reader), id);

            if (await _borrowRecordRepository.AnyAsync(r => r.ReaderId == id && r.ReturnedAt == null))
                throw new ConflictException("This reader still has borrowed books. They must be returned first.");

            _readerRepository.Delete(reader);
            await _readerRepository.SaveChangesAsync();
        }

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

        private async Task EnsureEmailIsFreeAsync(string email, int? exceptId = null)
        {
            if (await _readerRepository.AnyAsync(r => r.Email == email && r.Id != exceptId))
                throw new ConflictException($"A reader with email '{email}' already exists.");
        }
    }
}
