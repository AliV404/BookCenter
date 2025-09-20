using BookCenter.Application.DTOs.Authors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.Abstractions.Services
{
    public interface IAuthorService
    {
        Task<IEnumerable<GetAuthorItemDto>> GetAllAsync(int page, int take);
        Task<GetAuthorDto> GetByIdAsync(int id);
        Task CreateAsync(CreateAuthorDto authorDto);
        Task UpdateAsync(int id, UpdateAuthorDto authorDto);
        Task DeleteAsync(int id);
    }
}
