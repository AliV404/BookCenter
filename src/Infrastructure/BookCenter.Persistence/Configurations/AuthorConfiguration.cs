using BookCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookCenter.Persistence.Configurations
{
    public class AuthorConfiguration : IEntityTypeConfiguration<Author>
    {
        public void Configure(EntityTypeBuilder<Author> builder)
        {
            builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
            builder.Property(x => x.Biography).HasMaxLength(2000);
            builder.HasIndex(x => x.Name);
        }
    }
}
