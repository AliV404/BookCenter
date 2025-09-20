using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.DTOs.Readers
{
    public record GetReaderItemDto(int Id, string Name, string Surname);
}
