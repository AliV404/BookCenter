using BookCenter.Domain.Base;

namespace BookCenter.Domain.Entities
{
    public class Book : BaseNameableEntity
    {
        public string ISBN { get; set; } = null!;
        public int PublishYear { get; set; }

        /// <summary>How many physical copies the library owns.</summary>
        public int TotalCopies { get; set; }

        /// <summary>Copies currently on the shelf (TotalCopies minus active loans). Maintained by the borrow/return flow.</summary>
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
