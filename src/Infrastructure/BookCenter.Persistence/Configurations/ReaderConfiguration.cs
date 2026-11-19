using BookCenter.Domain.Entities;
using BookCenter.Persistence.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookCenter.Persistence.Configurations
{
    public class ReaderConfiguration : IEntityTypeConfiguration<Reader>
    {
        public void Configure(EntityTypeBuilder<Reader> builder)
        {
            builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
            builder.Property(r => r.Surname).IsRequired().HasMaxLength(100);
            builder.Property(r => r.Email).IsRequired().HasMaxLength(256);
            builder.Property(r => r.Phone).IsRequired().HasMaxLength(30);
            builder.HasIndex(r => r.Email).IsUnique().HasFilter("[IsDeleted] = 0");

            // Optional 1:1 link to a login account. No navigation property, so Domain stays Identity-free.
            builder.HasOne<AppUser>().WithOne().HasForeignKey<Reader>(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(r => r.UserId).IsUnique().HasFilter("[UserId] IS NOT NULL AND [IsDeleted] = 0");
        }
    }
}
