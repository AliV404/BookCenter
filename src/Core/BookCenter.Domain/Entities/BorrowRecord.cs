using BookCenter.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Domain.Entities
{
    public class BorrowRecord:BaseEntity
    {
        public int BookId { get; set; }
        public Book Book { get; set; }

        public int ReaderId { get; set; }
        public Reader Reader { get; set; }

        public DateTime BorrowedAt { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public DateTime? ReturnedAt { get; set; }

        public bool IsOverdue => ReturnedAt == null && DateTime.UtcNow > DueDate;
    }
}
