using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.DTOs.Books
{
    public record GetBookItemDto(int Id, string Name);
}
