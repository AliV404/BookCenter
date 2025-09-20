using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.DTOs.Authors
{
    public class CreateAuthorDto
    {
        public string Name { get; set; } = null!;
        public string Biography { get; set; } = null!;
    }
}
