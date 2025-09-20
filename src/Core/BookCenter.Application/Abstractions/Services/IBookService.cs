using BookCenter.Application.DTOs.Books;
using BookCenter.Application.DTOs.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.Abstractions.Services
{
    public interface IBookService
    {
        Task<IEnumerable<GetBookItemDto>> GetAllAsync(int page, int take);
        Task<GetBookDto> GetByIdAsync(int id);
        Task CreateAsync(CreateBookDto dto);
        Task UpdateAsync(int id, UpdateBookDto dto);
        Task DeleteAsync(int id);
    }
}
