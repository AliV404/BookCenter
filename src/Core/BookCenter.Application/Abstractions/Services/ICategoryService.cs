using BookCenter.Application.Common;
using BookCenter.Application.DTOs.Categories;

namespace BookCenter.Application.Abstractions.Services
{
    public interface ICategoryService
    {
        Task<PagedResult<GetCategoryItemDto>> GetAllAsync(PaginationQuery query);
        Task<GetCategoryDto> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateCategoryDto dto);
        Task UpdateAsync(int id, UpdateCategoryDto dto);
        Task DeleteAsync(int id);
    }
}
