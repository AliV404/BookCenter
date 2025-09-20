using BookCenter.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Domain.Entities
{
    public class Book:BaseNameableEntity
    {
        public string ISBN { get; set; } = null!;
        public int PublishYear { get; set; }
        public int CopiesAvailable { get; set; }

        // Relations
        public int AuthorId { get; set; }
        public Author Author { get; set; } = null!;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public int PublisherId { get; set; }
        public Publisher Publisher { get; set; } = null!;

        public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();

    }
}
