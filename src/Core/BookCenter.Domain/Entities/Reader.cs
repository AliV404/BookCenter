using BookCenter.Domain.Base;

namespace BookCenter.Domain.Entities
{
    public class Reader : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string Surname { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;

        /// <summary>Identity user id when this reader has an online account; null for desk-only readers.</summary>
        public string? UserId { get; set; }

        public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();
    }
}
