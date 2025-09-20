using BookCenter.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Domain.Entities
{
    public class Author:BaseNameableEntity
    {
        public string? Biography { get; set; }
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
