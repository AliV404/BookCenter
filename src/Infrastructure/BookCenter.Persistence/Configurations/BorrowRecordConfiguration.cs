using BookCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookCenter.Persistence.Configurations
{
    public class BorrowRecordConfiguration : IEntityTypeConfiguration<BorrowRecord>
    {
        public void Configure(EntityTypeBuilder<BorrowRecord> builder)
        {
            builder.Ignore(r => r.IsOverdue);

            builder.HasOne(r => r.Book).WithMany(b => b.BorrowRecords).HasForeignKey(r => r.BookId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(r => r.Reader).WithMany(r => r.BorrowRecords).HasForeignKey(r => r.ReaderId).OnDelete(DeleteBehavior.Restrict);

            // Speeds up "active loans" / "overdue" lookups.
            builder.HasIndex(r => new { r.ReturnedAt, r.DueDate });
        }
    }
}
