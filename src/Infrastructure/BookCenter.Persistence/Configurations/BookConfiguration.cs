using BookCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookCenter.Persistence.Configurations
{
    public class BookConfiguration : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> builder)
        {
            builder.Property(b => b.Name).IsRequired().HasMaxLength(200);
            builder.Property(b => b.ISBN).IsRequired().HasMaxLength(13).IsFixedLength();
            builder.HasIndex(b => b.ISBN).IsUnique().HasFilter("[IsDeleted] = 0");

            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Books_CopiesAvailable", "[CopiesAvailable] >= 0 AND [CopiesAvailable] <= [TotalCopies]");
            });

            // Optimistic concurrency: SQL Server bumps this on every UPDATE. If two requests try to
            // borrow the last copy at the same time, the second SaveChanges throws
            // DbUpdateConcurrencyException instead of silently making CopiesAvailable negative.
            builder.Property<byte[]>("RowVersion").IsRowVersion();

            // Restrict instead of Cascade: deleting an author/category/publisher must never wipe books.
            builder.HasOne(b => b.Author).WithMany(a => a.Books).HasForeignKey(b => b.AuthorId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(b => b.Category).WithMany(c => c.Books).HasForeignKey(b => b.CategoryId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(b => b.Publisher).WithMany(p => p.Books).HasForeignKey(b => b.PublisherId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
