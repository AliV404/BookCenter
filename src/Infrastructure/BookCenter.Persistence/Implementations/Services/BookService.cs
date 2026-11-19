using BookCenter.Application.Abstractions.Repositories;
using BookCenter.Application.Abstractions.Services;
using BookCenter.Application.Common;
using BookCenter.Application.DTOs.Books;
using BookCenter.Application.Exceptions;
using BookCenter.Domain.Entities;
using BookCenter.Persistence.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookCenter.Persistence.Implementations.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IPublisherRepository _publisherRepository;
        private readonly IBorrowRecordRepository _borrowRecordRepository;

        // A projection: EF turns this into a SELECT with JOINs and fetches only these columns.
        private static readonly Expression<Func<Book, GetBookDto>> ToDto = b => new GetBookDto(
            b.Id, b.Name, b.ISBN, b.PublishYear, b.TotalCopies, b.CopiesAvailable,
            b.AuthorId, b.Author.Name,
            b.CategoryId, b.Category.Name,
            b.PublisherId, b.Publisher.Name);

        public BookService(
            IBookRepository bookRepository,
            IAuthorRepository authorRepository,
            ICategoryRepository categoryRepository,
            IPublisherRepository publisherRepository,
            IBorrowRecordRepository borrowRecordRepository)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
            _categoryRepository = categoryRepository;
            _publisherRepository = publisherRepository;
            _borrowRecordRepository = borrowRecordRepository;
        }

        public async Task<PagedResult<GetBookDto>> GetAllAsync(BookQuery query)
        {
            var books = _bookRepository.GetAll();

            if (query.Search is not null)
                books = books.Where(b => b.Name.Contains(query.Search)
                                      || b.ISBN.Contains(query.Search)
                                      || b.Author.Name.Contains(query.Search));
            if (query.AuthorId is not null)
                books = books.Where(b => b.AuthorId == query.AuthorId);
            if (query.CategoryId is not null)
                books = books.Where(b => b.CategoryId == query.CategoryId);
            if (query.PublisherId is not null)
                books = books.Where(b => b.PublisherId == query.PublisherId);

            return await books
                .OrderBy(b => b.Name).ThenBy(b => b.Id)
                .Select(ToDto)
                .ToPagedResultAsync(query);
        }

        public async Task<GetBookDto> GetByIdAsync(int id)
        {
            return await _bookRepository.GetAll(b => b.Id == id).Select(ToDto).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Book), id);
        }

        public async Task<int> CreateAsync(CreateBookDto dto)
        {
            await EnsureReferencesExistAsync(dto.AuthorId, dto.CategoryId, dto.PublisherId);
            await EnsureIsbnIsFreeAsync(dto.ISBN);

            var book = new Book
            {
                Name = dto.Name,
                ISBN = dto.ISBN,
                PublishYear = dto.PublishYear,
                TotalCopies = dto.TotalCopies,
                CopiesAvailable = dto.TotalCopies,
                AuthorId = dto.AuthorId,
                CategoryId = dto.CategoryId,
                PublisherId = dto.PublisherId
            };

            await _bookRepository.AddAsync(book);
            await _bookRepository.SaveChangesAsync();
            return book.Id;
        }

        public async Task UpdateAsync(int id, UpdateBookDto dto)
        {
            var book = await _bookRepository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Book), id);

            await EnsureReferencesExistAsync(dto.AuthorId, dto.CategoryId, dto.PublisherId);
            await EnsureIsbnIsFreeAsync(dto.ISBN, exceptId: id);

            var onLoan = book.TotalCopies - book.CopiesAvailable;
            if (dto.TotalCopies < onLoan)
                throw new ConflictException($"{onLoan} copies are currently on loan, so total copies cannot be less than {onLoan}.");

            book.Name = dto.Name;
            book.ISBN = dto.ISBN;
            book.PublishYear = dto.PublishYear;
            book.TotalCopies = dto.TotalCopies;
            book.CopiesAvailable = dto.TotalCopies - onLoan;
            book.AuthorId = dto.AuthorId;
            book.CategoryId = dto.CategoryId;
            book.PublisherId = dto.PublisherId;

            try
            {
                await _bookRepository.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException("This book was changed by someone else (e.g. borrowed) while you were editing. Reload and try again.");
            }
        }

        public async Task DeleteAsync(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Book), id);

            if (await _borrowRecordRepository.AnyAsync(r => r.BookId == id && r.ReturnedAt == null))
                throw new ConflictException("This book has copies on loan. It can be deleted after they are returned.");

            _bookRepository.Delete(book);
            await _bookRepository.SaveChangesAsync();
        }

        private async Task EnsureReferencesExistAsync(int authorId, int categoryId, int publisherId)
        {
            if (!await _authorRepository.AnyAsync(a => a.Id == authorId))
                throw new NotFoundException(nameof(Author), authorId);
            if (!await _categoryRepository.AnyAsync(c => c.Id == categoryId))
                throw new NotFoundException(nameof(Category), categoryId);
            if (!await _publisherRepository.AnyAsync(p => p.Id == publisherId))
                throw new NotFoundException(nameof(Publisher), publisherId);
        }

        private async Task EnsureIsbnIsFreeAsync(string isbn, int? exceptId = null)
        {
            if (await _bookRepository.AnyAsync(b => b.ISBN == isbn && b.Id != exceptId))
                throw new ConflictException($"A book with ISBN {isbn} already exists.");
        }
    }
}
