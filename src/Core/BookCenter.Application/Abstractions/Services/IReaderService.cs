using BookCenter.Application.Common;
using BookCenter.Application.DTOs.Readers;

namespace BookCenter.Application.Abstractions.Services
{
    public interface IReaderService
    {
        Task<PagedResult<GetReaderDto>> GetAllAsync(PaginationQuery query);
        Task<GetReaderDto> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateReaderDto dto);
        Task UpdateAsync(int id, UpdateReaderDto dto);
        Task DeleteAsync(int id);
    }
}
