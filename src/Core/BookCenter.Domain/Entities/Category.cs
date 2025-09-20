using BookCenter.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Domain.Entities
{
    public class Category:BaseNameableEntity
    {
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
