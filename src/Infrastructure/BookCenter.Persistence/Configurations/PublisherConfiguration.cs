using BookCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookCenter.Persistence.Configurations
{
    public class PublisherConfiguration : IEntityTypeConfiguration<Publisher>
    {
        public void Configure(EntityTypeBuilder<Publisher> builder)
        {
            builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
            builder.HasIndex(x => x.Name).IsUnique().HasFilter("[IsDeleted] = 0");
        }
    }
}
