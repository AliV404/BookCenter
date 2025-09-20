using BookCenter.Application.DTOs.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.Abstractions.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<GetCategoyItemDto>> GetAllAsync(int page, int take);
        Task<GetCategoyDto> GetByIdAsync(int id);
        Task CreateAsync(CreateCategoyDto dto);
        Task UpdateAsync(int id, UpdateCategoryDto dto);
        Task DeleteAsync(int id);
    }
}
