using BookCenter.Application.DTOs.Books;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.DTOs.Authors
{
    public record GetAuthorDto(int Id, string Name, string Biography, IEnumerable<GetBookItemDto> Books);
}
