using BookCenter.Application.Abstractions.Repositories;
using BookCenter.Application.Abstractions.Services;
using BookCenter.Application.Common;
using BookCenter.Application.DTOs.Authors;
using BookCenter.Application.DTOs.Books;
using BookCenter.Application.Exceptions;
using BookCenter.Domain.Entities;
using BookCenter.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace BookCenter.Persistence.Implementations.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IBookRepository _bookRepository;

        public AuthorService(IAuthorRepository authorRepository, IBookRepository bookRepository)
        {
            _authorRepository = authorRepository;
            _bookRepository = bookRepository;
        }

        public async Task<PagedResult<GetAuthorItemDto>> GetAllAsync(PaginationQuery query)
        {
            var authors = _authorRepository.GetAll();
            if (query.Search is not null)
                authors = authors.Where(a => a.Name.Contains(query.Search));

            return await authors
                .OrderBy(a => a.Name).ThenBy(a => a.Id)
                .Select(a => new GetAuthorItemDto(a.Id, a.Name, a.Books.Count))
                .ToPagedResultAsync(query);
        }

        public async Task<GetAuthorDto> GetByIdAsync(int id)
        {
            return await _authorRepository.GetAll(a => a.Id == id)
                .Select(a => new GetAuthorDto(
                    a.Id,
                    a.Name,
                    a.Biography,
                    a.Books.OrderBy(b => b.Name).Select(b => new GetBookItemDto(b.Id, b.Name)).ToList()))
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Author), id);
        }

        public async Task<int> CreateAsync(CreateAuthorDto dto)
        {
            await EnsureNameIsFreeAsync(dto.Name);

            var author = new Author { Name = dto.Name, Biography = dto.Biography };
            await _authorRepository.AddAsync(author);
            await _authorRepository.SaveChangesAsync();
            return author.Id;
        }

        public async Task UpdateAsync(int id, UpdateAuthorDto dto)
        {
            var author = await _authorRepository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Author), id);

            await EnsureNameIsFreeAsync(dto.Name, exceptId: id);

            // The entity is tracked, so just changing properties is enough: no Update() call needed.
            author.Name = dto.Name;
            author.Biography = dto.Biography;
            await _authorRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var author = await _authorRepository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Author), id);

            if (await _bookRepository.AnyAsync(b => b.AuthorId == id))
                throw new ConflictException("This author still has books. Delete or reassign them first.");

            _authorRepository.Delete(author);
            await _authorRepository.SaveChangesAsync();
        }

        private async Task EnsureNameIsFreeAsync(string name, int? exceptId = null)
        {
            if (await _authorRepository.AnyAsync(a => a.Name == name && a.Id != exceptId))
                throw new ConflictException($"An author named '{name}' already exists.");
        }
    }
}
