using BookCenter.Domain.Entities;
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
        }
    }
}
