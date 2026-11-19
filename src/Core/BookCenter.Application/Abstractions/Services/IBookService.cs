using BookCenter.Application.Common;
using BookCenter.Application.DTOs.Books;

namespace BookCenter.Application.Abstractions.Services
{
    public interface IBookService
    {
        Task<PagedResult<GetBookDto>> GetAllAsync(BookQuery query);
        Task<GetBookDto> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateBookDto dto);
        Task UpdateAsync(int id, UpdateBookDto dto);
        Task DeleteAsync(int id);
    }
}
