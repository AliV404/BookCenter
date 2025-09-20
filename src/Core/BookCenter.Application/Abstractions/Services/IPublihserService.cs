using BookCenter.Application.DTOs.Authors;
using BookCenter.Application.DTOs.Publishers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.Abstractions.Services
{
    public interface IPublihserService
    {
        Task<IEnumerable<GetPublisherItemDto>> GetAllAsync(int page, int take);
        Task<GetPublisherDto> GetByIdAsync(int id);
        Task CreateAsync(CreatePublisherDto publisherDto);
        Task UpdateAsync(int id, UpdatePublisherDto publisherDto);
        Task DeleteAsync(int id);
    }
}
