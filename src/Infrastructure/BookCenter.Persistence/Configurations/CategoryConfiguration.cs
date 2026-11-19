using BookCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookCenter.Persistence.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);

            // Unique only among non-deleted rows, so a deleted category's name can be reused.
            builder.HasIndex(x => x.Name).IsUnique().HasFilter("[IsDeleted] = 0");
        }
    }
}
