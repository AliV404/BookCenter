using BookCenter.Application.Abstractions.Repositories;
using BookCenter.Application.Abstractions.Services;
using BookCenter.Application.Common;
using BookCenter.Application.DTOs.Readers;
using BookCenter.Application.Exceptions;
using BookCenter.Domain.Entities;
using BookCenter.Persistence.Common;
using BookCenter.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookCenter.Persistence.Implementations.Services
{
    public class ReaderService : IReaderService
    {
        private readonly IReaderRepository _readerRepository;
        private readonly IBorrowRecordRepository _borrowRecordRepository;
        private readonly UserManager<AppUser> _userManager;

        private static readonly Expression<Func<Reader, GetReaderDto>> ToDto = r => new GetReaderDto(
            r.Id, r.Name, r.Surname, r.Email, r.Phone,
            r.BorrowRecords.Count(br => br.ReturnedAt == null),
            r.UserId != null);

        // The repositories and UserManager share one scoped AppDbContext, so a single SaveChanges
        // (whichever of them triggers it) stores reader and account changes together.
        public ReaderService(IReaderRepository readerRepository, IBorrowRecordRepository borrowRecordRepository, UserManager<AppUser> userManager)
        {
            _readerRepository = readerRepository;
            _borrowRecordRepository = borrowRecordRepository;
            _userManager = userManager;
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

            // If a login account with this email has no active card (e.g. its card was deleted),
            // link the new card to it: this is how a deactivated reader is re-activated.
            var account = await _userManager.FindByEmailAsync(email);
            if (account is not null && !await _readerRepository.AnyAsync(r => r.UserId == account.Id))
                reader.UserId = account.Id;

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

            var emailChanged = reader.Email != email;
            reader.Name = dto.Name;
            reader.Surname = dto.Surname;
            reader.Email = email;
            reader.Phone = dto.Phone.Trim();

            if (reader.UserId is null)
            {
                await _readerRepository.SaveChangesAsync();
                return;
            }

            // Keep the linked login account in step: the reader signs in with their card's email.
            var account = await _userManager.FindByIdAsync(reader.UserId)
                ?? throw new InvalidOperationException($"Reader {id} is linked to a missing account.");

            if (emailChanged && await _userManager.FindByEmailAsync(email) is { } owner && owner.Id != account.Id)
                throw new ConflictException($"Another login account already uses '{email}'.");

            account.Email = email;
            account.UserName = email;
            account.FullName = $"{reader.Name} {reader.Surname}";

            // A new email signs the reader out everywhere (their token still names the old one).
            var saved = emailChanged
                ? await _userManager.UpdateSecurityStampAsync(account)
                : await _userManager.UpdateAsync(account);
            if (!saved.Succeeded)
                throw IdentityErrors.ToValidationException(saved.Errors);
        }

        public async Task DeleteAsync(int id)
        {
            var reader = await _readerRepository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Reader), id);

            if (await _borrowRecordRepository.AnyAsync(r => r.ReaderId == id && r.ReturnedAt == null))
                throw new ConflictException("This reader still has borrowed books. They must be returned first.");

            _readerRepository.Delete(reader);

            if (reader.UserId is not null && await _userManager.FindByIdAsync(reader.UserId) is { } account)
            {
                // Without a card a reader can't sign in; also end the sessions they already have.
                (await _userManager.UpdateSecurityStampAsync(account)).ThrowIfFailed("sign the reader out");
                return;
            }

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
