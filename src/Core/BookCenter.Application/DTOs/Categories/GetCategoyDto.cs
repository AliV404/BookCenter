using BookCenter.Application.DTOs.Books;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.DTOs.Categories
{
    public record GetCategoyDto(int Id, string Name, IEnumerable<GetBookItemDto> Books);
}
