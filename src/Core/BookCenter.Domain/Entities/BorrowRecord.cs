using BookCenter.Domain.Base;

namespace BookCenter.Domain.Entities
{
    public class BorrowRecord : BaseEntity
    {
        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        public int ReaderId { get; set; }
        public Reader Reader { get; set; } = null!;

        public DateTime BorrowedAt { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnedAt { get; set; }

        public bool IsOverdue => ReturnedAt == null && DateTime.UtcNow > DueDate;
    }
}
