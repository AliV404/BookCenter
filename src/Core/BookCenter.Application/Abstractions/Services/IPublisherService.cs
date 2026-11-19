using BookCenter.Application.Common;
using BookCenter.Application.DTOs.Publishers;

namespace BookCenter.Application.Abstractions.Services
{
    public interface IPublisherService
    {
        Task<PagedResult<GetPublisherItemDto>> GetAllAsync(PaginationQuery query);
        Task<GetPublisherDto> GetByIdAsync(int id);
        Task<int> CreateAsync(CreatePublisherDto dto);
        Task UpdateAsync(int id, UpdatePublisherDto dto);
        Task DeleteAsync(int id);
    }
}
