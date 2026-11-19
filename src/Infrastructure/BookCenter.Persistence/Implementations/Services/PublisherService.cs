using BookCenter.Application.Abstractions.Repositories;
using BookCenter.Application.Abstractions.Services;
using BookCenter.Application.Common;
using BookCenter.Application.DTOs.Books;
using BookCenter.Application.DTOs.Publishers;
using BookCenter.Application.Exceptions;
using BookCenter.Domain.Entities;
using BookCenter.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace BookCenter.Persistence.Implementations.Services
{
    public class PublisherService : IPublisherService
    {
        private readonly IPublisherRepository _publisherRepository;
        private readonly IBookRepository _bookRepository;

        public PublisherService(IPublisherRepository publisherRepository, IBookRepository bookRepository)
        {
            _publisherRepository = publisherRepository;
            _bookRepository = bookRepository;
        }

        public async Task<PagedResult<GetPublisherItemDto>> GetAllAsync(PaginationQuery query)
        {
            var publishers = _publisherRepository.GetAll();
            if (query.Search is not null)
                publishers = publishers.Where(p => p.Name.Contains(query.Search));

            return await publishers
                .OrderBy(p => p.Name).ThenBy(p => p.Id)
                .Select(p => new GetPublisherItemDto(p.Id, p.Name, p.Books.Count))
                .ToPagedResultAsync(query);
        }

        public async Task<GetPublisherDto> GetByIdAsync(int id)
        {
            return await _publisherRepository.GetAll(p => p.Id == id)
                .Select(p => new GetPublisherDto(
                    p.Id,
                    p.Name,
                    p.Books.OrderBy(b => b.Name).Select(b => new GetBookItemDto(b.Id, b.Name)).ToList()))
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Publisher), id);
        }

        public async Task<int> CreateAsync(CreatePublisherDto dto)
        {
            await EnsureNameIsFreeAsync(dto.Name);

            var publisher = new Publisher { Name = dto.Name };
            await _publisherRepository.AddAsync(publisher);
            await _publisherRepository.SaveChangesAsync();
            return publisher.Id;
        }

        public async Task UpdateAsync(int id, UpdatePublisherDto dto)
        {
            var publisher = await _publisherRepository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Publisher), id);

            await EnsureNameIsFreeAsync(dto.Name, exceptId: id);

            publisher.Name = dto.Name;
            await _publisherRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var publisher = await _publisherRepository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Publisher), id);

            if (await _bookRepository.AnyAsync(b => b.PublisherId == id))
                throw new ConflictException("This publisher still has books. Move or delete them first.");

            _publisherRepository.Delete(publisher);
            await _publisherRepository.SaveChangesAsync();
        }

        private async Task EnsureNameIsFreeAsync(string name, int? exceptId = null)
        {
            if (await _publisherRepository.AnyAsync(p => p.Name == name && p.Id != exceptId))
                throw new ConflictException($"A publisher named '{name}' already exists.");
        }
    }
}
