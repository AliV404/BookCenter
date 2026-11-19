using BookCenter.Application.Abstractions.Repositories;
using BookCenter.Application.Abstractions.Services;
using BookCenter.Application.Common;
using BookCenter.Application.DTOs.Books;
using BookCenter.Application.DTOs.Categories;
using BookCenter.Application.Exceptions;
using BookCenter.Domain.Entities;
using BookCenter.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace BookCenter.Persistence.Implementations.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBookRepository _bookRepository;

        public CategoryService(ICategoryRepository categoryRepository, IBookRepository bookRepository)
        {
            _categoryRepository = categoryRepository;
            _bookRepository = bookRepository;
        }

        public async Task<PagedResult<GetCategoryItemDto>> GetAllAsync(PaginationQuery query)
        {
            var categories = _categoryRepository.GetAll();
            if (query.Search is not null)
                categories = categories.Where(c => c.Name.Contains(query.Search));

            return await categories
                .OrderBy(c => c.Name).ThenBy(c => c.Id)
                .Select(c => new GetCategoryItemDto(c.Id, c.Name, c.Books.Count))
                .ToPagedResultAsync(query);
        }

        public async Task<GetCategoryDto> GetByIdAsync(int id)
        {
            return await _categoryRepository.GetAll(c => c.Id == id)
                .Select(c => new GetCategoryDto(
                    c.Id,
                    c.Name,
                    c.Books.OrderBy(b => b.Name).Select(b => new GetBookItemDto(b.Id, b.Name)).ToList()))
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Category), id);
        }

        public async Task<int> CreateAsync(CreateCategoryDto dto)
        {
            await EnsureNameIsFreeAsync(dto.Name);

            var category = new Category { Name = dto.Name };
            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();
            return category.Id;
        }

        public async Task UpdateAsync(int id, UpdateCategoryDto dto)
        {
            var category = await _categoryRepository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Category), id);

            await EnsureNameIsFreeAsync(dto.Name, exceptId: id);

            category.Name = dto.Name;
            await _categoryRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Category), id);

            if (await _bookRepository.AnyAsync(b => b.CategoryId == id))
                throw new ConflictException("This category still has books. Move or delete them first.");

            _categoryRepository.Delete(category);
            await _categoryRepository.SaveChangesAsync();
        }

        private async Task EnsureNameIsFreeAsync(string name, int? exceptId = null)
        {
            if (await _categoryRepository.AnyAsync(c => c.Name == name && c.Id != exceptId))
                throw new ConflictException($"A category named '{name}' already exists.");
        }
    }
}
