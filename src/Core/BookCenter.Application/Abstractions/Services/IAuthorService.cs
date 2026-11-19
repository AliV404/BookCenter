using BookCenter.Application.Common;
using BookCenter.Application.DTOs.Authors;

namespace BookCenter.Application.Abstractions.Services
{
    public interface IAuthorService
    {
        Task<PagedResult<GetAuthorItemDto>> GetAllAsync(PaginationQuery query);
        Task<GetAuthorDto> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateAuthorDto dto);
        Task UpdateAsync(int id, UpdateAuthorDto dto);
        Task DeleteAsync(int id);
    }
}
